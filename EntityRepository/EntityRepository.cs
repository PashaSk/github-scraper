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
        }

        public async Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel filter, CancellationToken ctoken = default)
        {
            ListSearchResult<FileEntity> result;
            using (var scope = _services.CreateScope())
            {
                var context =
                 scope.ServiceProvider
                     .GetRequiredService<IGithubContext>();

                result = await context.GetFilesAsync(filter, ctoken).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel filter, CancellationToken ctoken = default)
        {
            ListSearchResult<TermEntity> result;
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<IGithubContext>();

                result = await context.GetTermsAsync(filter, ctoken).ConfigureAwait(false);
            }

            return result;
        }

        public async Task SaveEntitiesBatch(IEnumerable<TermEntity> terms, FileEntity file, CancellationToken ctoken = default)
        {
            using (var scope = _services.CreateScope())
            {
                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IGithubContext>();

                await context.SaveTermsAsync(file, terms, ctoken).ConfigureAwait(false);
            }
        }

        public async Task<FileEntity> GetFileAsync(SearchModel filter, CancellationToken ctoken = default)
        {
            FileEntity result;
            using (var scope = _services.CreateScope())
            {
                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IGithubContext>();

                result = await context.GetFileAsync(filter, ctoken).ConfigureAwait(false);
            }

            return result;
        }
        public async Task<TermEntity> GetTermAsync(SearchModel filter, CancellationToken ctoken = default)
        {
            TermEntity result;
            using (var scope = _services.CreateScope())
            {
                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IGithubContext>();

                result = await context.GetTermAsync(filter, ctoken).ConfigureAwait(false);
            }

            return result;
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
