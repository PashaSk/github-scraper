using ClassScraper.AspGithubClassScraper.Config;
using Microsoft.Extensions.Hosting;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Unicode;
using System.Net.Http;
using ClassScraper.DomainObjects.Github;
using ClassScraper.AspGithubClassScraper.Utils;
using ClassScraper.Repository.EntityService;
using Polly.Retry;
using Polly.Extensions.Http;
using System.Net.Sockets;
using Polly;
using System.IO;

namespace ClassScraper.AspGithubClassScraper.Worker
{
    public class GithubFilesScraper : BackgroundService
    {
        private GithubConfig _githubConfig;
        private GitHubClient _githubClient;
        private ILogger<GithubFilesScraper> _logger;
        private Timer _timer;
        private IHostApplicationLifetime _app;
        private IAspTermsExtractor _aspTermsExtractor;
        private IEntityRepository _repository;
        private CancellationToken _cancelationToken;
        private int _dirsRemaining;
        private int _filesRemaining;
        private AsyncRetryPolicy<byte[]> _retryPolicy;
        public GithubFilesScraper(GithubConfig githubConfig,
                                  ILogger<GithubFilesScraper> logger,
                                  IHostApplicationLifetime appLifetime,
                                  IAspTermsExtractor aspTermsExtractor,
                                  IEntityRepository repository)
        {
            _githubConfig = githubConfig;
            _logger = logger;
            _githubClient = new GitHubClient(new ProductHeaderValue(_githubConfig.UserName));
            _githubClient.Connection.SetRequestTimeout(TimeSpan.FromSeconds(20));
            _githubClient.Credentials = new Credentials(_githubConfig.UserName, _githubConfig.Token);
            _app = appLifetime;
            _aspTermsExtractor = aspTermsExtractor;
            _repository = repository;
            _retryPolicy = Policy<byte[]>
                .Handle<TaskCanceledException>(e => e.InnerException is IOException)
                .Or<TaskCanceledException>(e => e.InnerException is SocketException)
                .Or<SocketException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30)
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogInformation("Retrying socket exception");
                });
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _cancelationToken = cancellationToken;
            _dirsRemaining = 1;
            _filesRemaining = 0;
            _logger.LogInformation("Begin Request Cycle");
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _timer = new Timer(WriteClientStatistics, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                while (!stoppingToken.IsCancellationRequested)
                {
                    await VisitDirectory();
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ExecuteAsync: exception occures");
            }

            WriteClientStatistics(null);
            _app.StopApplication();
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ending Request Cycle");
            _logger.LogInformation("GithubFilesScraper service stopped");

            _timer.Dispose();
            return Task.FromResult(0);
        }

        private async Task VisitDirectory(string path = null)
        {
            IReadOnlyList<RepositoryContent> content;
            if (_githubConfig.DirectoryBlackList.Contains(path))
            {
                _logger.LogInformation($"ignoring directory {path}");
                return;
            }
            else
            {
                _logger.LogInformation($"visiting directory {path}");
            }

            if (path == null)
            {
                content = await _githubClient.Repository.Content.GetAllContents(_githubConfig.RepositoryOwnerName, _githubConfig.RepositoryName);
            }
            else
            {
                content = await _githubClient.Repository.Content.GetAllContents(_githubConfig.RepositoryOwnerName, _githubConfig.RepositoryName, path);
            }


            var filtered = content
                .Where(p => p.Type == ContentType.Dir || p.Type == ContentType.File)
                .Where(p => p.Type == ContentType.Dir ? _githubConfig.DirectoryBlackList.Contains(path) == false : p.Name.EndsWith(".cs"));

            _dirsRemaining += filtered.Count(r => r.Type == ContentType.Dir);
            _filesRemaining += filtered.Count(r => r.Type == ContentType.File);

            foreach (var entity in filtered)
            {
                if (entity.Type == ContentType.File)
                {
                    await ProcessFile(entity);
                }
                else if (entity.Type == ContentType.Dir)
                {
                    await VisitDirectory(entity.Path);
                }
            }
            _dirsRemaining -= 1;
        }

        private async Task ProcessFile(RepositoryContent file)
        {
            _logger.LogInformation($"visiting file {file.Path}");
            _logger.LogInformation($"{file.DownloadUrl}");

            var response = await _retryPolicy.ExecuteAsync(async () => await _githubClient.Repository.Content.GetRawContent(_githubConfig.RepositoryOwnerName, _githubConfig.RepositoryName, file.Path));
            var content = Encoding.UTF8.GetString(response);
            var fileEntity = new FileEntity()
            {
                ID = Guid.NewGuid(),
                Name = file.Name,
                Path = file.Path,
                Url = file.GitUrl,
                HtmlUrl = file.HtmlUrl,
                OwnerName = _githubConfig.RepositoryOwnerName,
                RepositoryName = _githubConfig.RepositoryName
            };
            var terms = _aspTermsExtractor.ExtractFileTerms(content);

            _logger.LogInformation($"Found Terms: {string.Join(',', terms.Select(t => $"{t.Name} : {t.TermType}"))}");
            if (terms.Count() == 0)
            {
                _logger.LogInformation($"Skipping {file.Name} as there is no suitable terms");
            }
            else
            {
                foreach (var term in terms)
                {
                    term.FileEntity = fileEntity;
                }

                try
                {
                    await _repository.SaveEntitiesBatch(terms, fileEntity, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("========== Exception ============");
                    Console.WriteLine(file.HtmlUrl);
                    Console.WriteLine(e.ToString());
                    throw;
                }

            }

            _filesRemaining -= 1;
        }

        private void WriteClientStatistics(object state)
        {
            var stats = _githubClient.GetLastApiInfo();
            Console.WriteLine($"Remaining dirs: {_dirsRemaining} files: {_filesRemaining} Client Stats Remaining: {stats?.RateLimit?.Remaining}");
        }
    }
}
