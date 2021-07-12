using ClassScraper.DomainObjects.Github;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DbLayer.PostgreService.Models
{
    class PostgreFileEntity
    {
        [Key]
        public string ID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }        
        public string Path { get; set; }
        public string Url { get; set; }
        public string HtmlUrl { get; set; }
        public string OwnerName { get; set; }
        public string RepositoryName { get; set; }

        public FileEntity ToDomain()
        {
            return new FileEntity()
            {
                ID = Guid.Parse(ID),
                Name = Name,
                Path = Path,
                Url = Url,
                HtmlUrl = HtmlUrl,
                OwnerName = OwnerName,
                RepositoryName = RepositoryName
            };
        }
        public PostgreFileEntity(FileEntity fileEntity)
        {
            ID = fileEntity.ID.ToString();
            Name = fileEntity.Name;
            Path = fileEntity.Path;
            Url = fileEntity.Url;
            HtmlUrl = fileEntity.HtmlUrl;
            OwnerName = fileEntity.OwnerName;
            RepositoryName = fileEntity.RepositoryName;
        }
        public PostgreFileEntity() {}
    }
}
