using ClassScraper.DomainObjects.Github;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ClassScraper.DbLayer.MongoDb.Models
{
    [BsonIgnoreExtraElements]
    class MongoTermEntity : IMongoEntity<TermEntity>
    {
        [BsonElement("ID")]
        public string ID { get; set; }

        [BsonElement("n")]
        public string Name { get; set; }
        [BsonElement("t")]
        public TermType TermType { get; set; }
        [BsonElement("f")]
        public string FileEntityId { get; set; }

        [BsonIgnore]
        public FileEntity FileEntity { get; set; }

        public TermEntity ToDomain()
        {
            return new TermEntity()
            {
                ID = Guid.Parse(ID),
                Name = Name,
                TermType = TermType,
                FileEntity = FileEntity
            };
        }
        public MongoTermEntity(TermEntity termEntity)
        {
            ID = termEntity.ID.ToString();
            Name = termEntity.Name;
            TermType = termEntity.TermType;
            FileEntityId = termEntity.FileEntity.ID.ToString();
        }
    }
}
