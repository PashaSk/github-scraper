using ClassScraper.DbLayer.MongoDb;
using ClassScraper.DbLayer.PostgreService;
using ClassScraper.DomainObjects.API;
using ClassScraper.DomainObjects.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClassScraper.Repository.EntityService
{
    public class EntityRepository : IEntityRepository
    {
        private IServiceProvider _services;

        public EntityRepository(
            IServiceProvider services
            )
        {
            _services = services;
            //using (var scope = _services.CreateScope())
            //{
            //    var context =
            //        scope.ServiceProvider
            //            .GetRequiredService<GithubContext>();

            //    var file = new FileEntity()
            //    {
            //        Name = "test",
            //        OwnerName = "owner-test",
            //        RepositoryName = "repo-test",
            //        Path = "",
            //        Url = ""
            //    };
            //    context.Files.Add(file);
            //    var newFile = new FileEntity()
            //    {
            //        Name = "test2",
            //        OwnerName = "owner-test2",
            //        RepositoryName = "repo-test2",
            //        Path = "",
            //        Url = ""
            //    };
            //    var term = new TermEntity()
            //    {
            //        Name = "Term Name",
            //        FileEntity = newFile,
            //        TermType = TermType.Class
            //    };
            //    var term2 = new TermEntity()
            //    {
            //        Name = "Term Name 2",
            //        FileEntity = newFile,
            //        TermType = TermType.Interface
            //    };
            //    context.Files.Add(newFile);
            //    context.Terms.Add(term);
            //    context.Terms.Add(term2);

            //    context.SaveChanges();

            //    foreach(var t in context.Terms)
            //    {
            //        Console.WriteLine(t.Name);
            //        Console.WriteLine(t.TermType);
            //    }
            //}
        }

        public async Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel filter)
        {
            ListSearchResult<FileEntity> result;
            using (var scope = _services.CreateScope())
            {
                var context =
                 scope.ServiceProvider
                     .GetRequiredService<IGithubContext>();

                result = await context.GetFilesAsync(filter);
            }

            return result;
        }

        public async Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel filter)
        {
            ListSearchResult<TermEntity> result;
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<IGithubContext>();

                result = await context.GetTermsAsync(filter);
            }

            return result;
        }

        public async Task SaveEntitiesBatch(IEnumerable<TermEntity> terms, FileEntity file, CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IGithubContext>();

                await context.SaveTermsAsync(file, terms, cancellationToken);
            }
        }
        private PostgreGithubContext GetDbContext()
        {
            PostgreGithubContext context;
            using (var scope = _services.CreateScope())
            {
                context =
                    scope.ServiceProvider
                        .GetRequiredService<PostgreGithubContext>();
            }

            return context;
        }
    }

    public static class ServiceCollectionExtensions
    {

        public static void AddEntityService(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            if (configuration.GetConnectionString("Mongo") != null)
            {
                logger.Information("Init storage service with MongoDB {value}", configuration.GetConnectionString("Mongo"));
                services.AddMongoDb(configuration);
            }
            else
            {
                logger.Information("Init storage service with PostgreSQL {value}", configuration.GetConnectionString("Postgre"));
                services.AddPostgreSql(configuration);
            }
            services.AddSingleton<IEntityRepository, EntityRepository>();
        }
        public static void CheckIndexes(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            if (configuration.GetConnectionString("Mongo") != null)
            {
                logger.Information("Creating mongo indexes");
                services.AddMongoIndexes(configuration, logger);
            }

            //TODO: postgre indexes
        }
    }
}
