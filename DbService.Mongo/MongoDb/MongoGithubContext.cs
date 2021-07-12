using ClassScraper.DomainObjects.Github;
using MongoDB.Driver;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ClassScraper.DbLayer.MongoDb.Models;
using MongoDB.Bson;
using Serilog;
using ClassScraper.DomainObjects.API;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ClassScraper.DbLayer.MongoDb
{
    public class MongoGithubContext : IGithubContext
    {
        private IMongoCollection<MongoFileEntity> files;
        private IMongoCollection<MongoTermEntity> terms;
        private const int MONGO_TIMEOUT_MS = 1500;
        private const int PAGE_SIZE = 5;

        public const string FILES_COLLECTION = "files";
        public const string TERMS_COLLECTION = "terms";

        public MongoGithubContext(string connectionString)
        {
            var db = new MongoClient(connectionString).GetDatabase("scraper");
            files = db.GetCollection<MongoFileEntity>("files");
            terms = db.GetCollection<MongoTermEntity>("terms");
        }
        public async Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var filter = Builders<MongoFileEntity>.Filter.Empty;
            if (search.FilterName != null)
            {
                filter = Builders<MongoFileEntity>.Filter.Regex(r => r.Name, new MongoDB.Bson.BsonRegularExpression(search.FilterName, "gi"));
            }

            var pipeline1 = PipelineDefinition<MongoFileEntity, BsonDocument>.Create(
                new BsonDocument() { { "$skip", search.Page * PAGE_SIZE } },
                new BsonDocument() { { "$limit", PAGE_SIZE } }
                );
            var pipeline2 = PipelineDefinition<MongoFileEntity, BsonDocument>.Create(
                    "{ $count : 'count'}"
                );

            var facet1 = AggregateFacet.Create("data", pipeline1);
            var facet2 = AggregateFacet.Create("totalCount", pipeline2);

            var result = await WithTimeoutAsync(async () => await files.Aggregate()
                .Match(filter)
                .Facet<BsonDocument>(new[] { facet1, facet2 })
                .AppendStage<BsonDocument>("{ $addFields: { total : { $arrayElemAt:[\"$totalCount.count\",0]}}}")
                .AppendStage<MongoListSearchResult<MongoFileEntity, FileEntity>>("{ $unset:  \"totalCount\" }").SingleAsync());

            return result.ToDomain();
        }

        public async Task<IEnumerable<TermEntity>> GetTermsAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var filter = Builders<MongoTermEntity>.Filter.Empty;
            if (search.FilterName != null)
            {
                filter = Builders<MongoTermEntity>.Filter.Regex(r => r.Name, new MongoDB.Bson.BsonRegularExpression(search.FilterName, "gi"));
            }
            var termEntities = await WithTimeoutAsync(async () => await terms.Find(filter).Skip(1 * PAGE_SIZE).ToListAsync());

            var ids = termEntities.Select(r => r.FileEntityId).ToArray();
            var filesEntities = await WithTimeoutAsync(async () => await files.Find(Builders<MongoFileEntity>.Filter.In(r => r.ID, ids)).ToListAsync());

            return termEntities.Select(e =>
            {
                var file = filesEntities.FirstOrDefault(r => r.ID == e.FileEntityId)?.ToDomain();
                e.FileEntity = file;
                return e.ToDomain();
            });
        }

        public async Task<bool> SaveTermsAsync(FileEntity file, IEnumerable<TermEntity> termsInput, CancellationToken ctoken = default)
        {
            var filesInsert = new MongoFileEntity(file);
            var termsInsert = termsInput.Select(t => new MongoTermEntity(t));

            await Task.WhenAll(files.InsertOneAsync(filesInsert, null, ctoken), terms.InsertManyAsync(termsInsert, null, ctoken));
            return true;
        }

        public Task<FileEntity> GetFileAsync(string file_id, CancellationToken ctoken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<TResult> WithTimeoutAsync<TResult>(Func<Task<TResult>> task, int timeout = MONGO_TIMEOUT_MS)
        {
            TResult result;
            try
            {
                using (var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout)))
                {
                    result = await task();
                }

                return result;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Operation was cancelled after {timeout} ms");
                throw;
            }
        }

        Task<ListSearchResult<TermEntity>> IGithubContext.GetTermsAsync(SearchModel search, CancellationToken ctoken)
        {
            throw new NotImplementedException();
        }
    }
    public static class ServiceCollectionExtensions
    {
        private const string ID_INDEX = "id_domain";
        private const string TEXT_INDEX = "text_domain";
        public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IGithubContext>(_ =>
            {
                return new MongoGithubContext(configuration.GetConnectionString("Mongo"));
            });
        }
        public async static void AddMongoIndexes(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            var db = new MongoClient(configuration.GetConnectionString("Mongo")).GetDatabase("scraper");

            await AddTermsIndexes(db, logger);
            await AddFilesIndexes(db, logger);
        }

        private async static Task<bool> AddTermsIndexes(IMongoDatabase db, ILogger logger)
        {
            var toCreate = new List<CreateIndexModel<MongoTermEntity>>();
            var terms = db.GetCollection<MongoTermEntity>(MongoGithubContext.TERMS_COLLECTION);
            var indexes = await terms.Indexes.ListAsync();
            var tersmList = indexes.ToList();

            if (tersmList.Any(b => b["name"] == ID_INDEX) == false)
            {
                toCreate.Add(new CreateIndexModel<MongoTermEntity>(Builders<MongoTermEntity>.IndexKeys.Ascending(i => i.ID), new CreateIndexOptions() { Name = ID_INDEX }));
            }
            if (tersmList.Any(b => b["name"] == TEXT_INDEX) == false)
            {
                toCreate.Add(new CreateIndexModel<MongoTermEntity>(Builders<MongoTermEntity>.IndexKeys.Ascending(i => i.Name), new CreateIndexOptions() { Name = TEXT_INDEX }));
            }

            if (toCreate.Count() > 0)
            {
                logger.Information("AddTermsIndexes -> Will create indexes {value}", toCreate.Select(t => t.Options.Name));
                await terms.Indexes.CreateManyAsync(toCreate);
            }

            return true;
        }
        private async static Task<bool> AddFilesIndexes(IMongoDatabase db, ILogger logger)
        {
            var files = db.GetCollection<MongoFileEntity>(MongoGithubContext.FILES_COLLECTION);
            var indexes = await files.Indexes.ListAsync();
            var filesList = indexes.ToList();

            if (filesList.Any(b => b["name"] == ID_INDEX) == false)
            {
                logger.Information("AddFilesIndexes -> Will create indexes {value}", ID_INDEX);
                await files.Indexes.CreateOneAsync(new CreateIndexModel<MongoFileEntity>(Builders<MongoFileEntity>.IndexKeys.Ascending(i => i.ID), new CreateIndexOptions() { Name = ID_INDEX }));
            }

            return true;
        }
    }
}

