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
            var filter = BuildFileFilter(search);
            var facets = GetFacets<MongoFileEntity>(search);

            var result = await WithTimeoutAsync(async () => await files.Aggregate()
                .Match(filter)
                .Facet<BsonDocument>(facets)
                .AppendStage<BsonDocument>("{ $addFields: { total : { $arrayElemAt:[\"$totalCount.count\",0]}}}")
                .AppendStage<MongoListSearchResult<MongoFileEntity, FileEntity>>("{ $unset:  \"totalCount\" }").SingleAsync(ctoken));

            return result.ToDomain();
        }

        public async Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var filter = BuildTermFilter(search);
            var facets = GetFacets<MongoTermEntity>(search);

            var result = await WithTimeoutAsync(async () => await terms.Aggregate()
                .Match(filter)
                .Facet<BsonDocument>(facets)
                .AppendStage<BsonDocument>("{ $addFields: { total : { $arrayElemAt:[\"$totalCount.count\",0]}}}")
                .AppendStage<MongoListSearchResult<MongoTermEntity, TermEntity>>("{ $unset:  \"totalCount\" }").SingleAsync(ctoken));

            var fileId = result.Data.Select(r => r.FileEntityId).ToHashSet();
            var filesList = await WithTimeoutAsync(async () => await this.files.Find(f => fileId.Contains(f.ID)).ToListAsync(ctoken));

            foreach (var term in result.Data)
            {
                term.FileEntity = filesList.FirstOrDefault(f => f.ID == term.FileEntityId)?.ToDomain();
            }

            return result.ToDomain();
        }

        public async Task<bool> SaveTermsAsync(FileEntity file, IEnumerable<TermEntity> termsInput, CancellationToken ctoken = default)
        {
            var filesInsert = new MongoFileEntity(file);
            var termsInsert = termsInput.Select(t => new MongoTermEntity(t));

            await Task.WhenAll(files.InsertOneAsync(filesInsert, null), terms.InsertManyAsync(termsInsert, null, ctoken));
            return true;
        }
        public async Task<FileEntity> GetFileAsync(SearchModel search, CancellationToken ctoken = default)
        {
            FileEntity result = null;
            var filter = BuildFileFilter(search);
            var file = await WithTimeoutAsync(async () => await files.Find(filter).FirstOrDefaultAsync(ctoken));
            if (file != null)
            {
                var terms = await WithTimeoutAsync(async () => await this.terms.Find(t => t.FileEntityId == file.ID).ToListAsync(ctoken));

                result = file.ToDomain();
                result.Terms = terms.Select(t => t.ToDomain());
            }

            return result;
        }

        public async Task<TermEntity> GetTermAsync(SearchModel search, CancellationToken ctoken = default)
        {
            TermEntity result = null;
            var filter = BuildTermFilter(search);
            var term = await WithTimeoutAsync(async () => await terms.Find(filter).FirstOrDefaultAsync(ctoken));
            if (term != null)
            {
                var file = await WithTimeoutAsync(async () => await files.Find(f => f.ID == term.FileEntityId).FirstAsync(ctoken));
                term.FileEntity = file?.ToDomain();
                result = term.ToDomain();
            }
            return result;
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

        private FilterDefinition<MongoFileEntity> BuildFileFilter(SearchModel search)
        {
            var filterAnd = new List<FilterDefinition<MongoFileEntity>>() { Builders<MongoFileEntity>.Filter.Empty };

            if (search.FilterId != null)
            {
                filterAnd.Add(Builders<MongoFileEntity>.Filter.Where(r => r.ID == search.FilterId));
            }
            if (search.FilterName != null)
            {
                filterAnd.Add(Builders<MongoFileEntity>.Filter.Regex(r => r.Name, new BsonRegularExpression(search.FilterName, "gi")));
            }

            return Builders<MongoFileEntity>.Filter.And(filterAnd);
        }
        private FilterDefinition<MongoTermEntity> BuildTermFilter(SearchModel search)
        {
            var filterAnd = new List<FilterDefinition<MongoTermEntity>>() { Builders<MongoTermEntity>.Filter.Empty };

            if (search.FilterName != null)
            {
                filterAnd.Add(Builders<MongoTermEntity>.Filter.Regex(r => r.Name, new BsonRegularExpression(search.FilterName, "gi")));
            }
            if (search.FilterId != null)
            {
                filterAnd.Add(Builders<MongoTermEntity>.Filter.Where(r => r.ID == search.FilterId));
            }
            if (search.TermType != TermType.Undefined)
            {
                filterAnd.Add(Builders<MongoTermEntity>.Filter.Where(r => r.TermType == search.TermType));
            }
            if (search.FilterFileId != null)
            {
                filterAnd.Add(Builders<MongoTermEntity>.Filter.Where(r => r.FileEntityId == search.FilterFileId));
            }

            return Builders<MongoTermEntity>.Filter.And(filterAnd);
        }
        private AggregateFacet<T>[] GetFacets<T>(SearchModel search)
        {
            var pipeline1 = PipelineDefinition<T, BsonDocument>.Create(
                new BsonDocument() { { "$skip", search.Page * PAGE_SIZE } },
                new BsonDocument() { { "$limit", PAGE_SIZE } }
                );
            var pipeline2 = PipelineDefinition<T, BsonDocument>.Create(
                    "{ $count : 'count'}"
                );

            var facet1 = AggregateFacet.Create("data", pipeline1);
            var facet2 = AggregateFacet.Create("totalCount", pipeline2);

            return new[] { facet1, facet2 };
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

