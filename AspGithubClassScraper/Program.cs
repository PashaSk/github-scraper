using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ClassScraper.AspGithubClassScraper.Worker;
using ClassScraper.AspGithubClassScraper.Config;
using ClassScraper.AspGithubClassScraper.Utils;
using ClassScraper.Repository.EntityService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit.Internal;
using Serilog;
using Microsoft.Extensions.Configuration;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly;
using System.Net.Sockets;

namespace ClassScraper.AspGithubClassScraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($"log-{timestamp}.txt")
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;
                    
                    services.AddSingleton<GithubConfig>();                    
                    services.AddTransient<IAspTermsExtractor, AspTermsExtractor>();
                    services.AddEntityService(config, Log.Logger);
                    services.AddHostedService<GithubFilesScraper>();

                });
    }
}
