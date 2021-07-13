using ClassScraper.DomainObjects.Github;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScraper.DbLayer.MongoDb.Models
{
    [BsonIgnoreExtraElements]
    class MongoFileEntity : IMongoEntity<FileEntity>
    {
        [BsonElement("ID")]
        public string ID { get; private set; }
        [BsonElement("n")]
        public string Name { get; set; }
        [BsonElement("p")]
        public string Path { get; set; }
        [BsonElement("u")]
        public string Url { get; set; }
        [BsonElement("hu")]
        public string HtmlUrl { get; set; }
        [BsonElement("on")]
        public string OwnerName { get; set; }
        [BsonElement("rn")]
        public string RepositoryName { get; set; }

        [BsonIgnore]
        public IEnumerable<TermEntity> Terms { get; set; } = new List<TermEntity>();
        public FileEntity ToDomain()
        {
            return new FileEntity()
            {
                ID = Guid.Parse(ID),
                Name = Name,
                Path = Path,
                Url = Url,
                OwnerName = OwnerName,
                RepositoryName = RepositoryName,
                HtmlUrl = HtmlUrl
            };
        }        
     
        public MongoFileEntity(FileEntity fileEntity)
        {
            ID = fileEntity.ID.ToString();
            Name = fileEntity.Name;
            Path = fileEntity.Path;
            Url = fileEntity.Url;
            HtmlUrl = fileEntity.HtmlUrl;
            OwnerName = fileEntity.OwnerName;
            RepositoryName = fileEntity.RepositoryName;
        }
    }
}
