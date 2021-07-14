using ClassScraper.DomainObjects.Github;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace ClassScraper.DbLayer.MongoDb.Models
{

    [BsonIgnoreExtraElements]
    public class MongoListSearchResult<T, D> where T : IMongoEntity<D>
    {
        [BsonElement("data")]
        public IEnumerable<T> Data { get; set; }
        [BsonElement("total")]
        public int TotalCount { get; set; }

        public ListSearchResult<D> ToDomain()
        {
            return new ListSearchResult<D>()
            {
                Data = Data.Select(d => d.ToDomain()),
                TotalCount = TotalCount
            };
        }
    }
}
