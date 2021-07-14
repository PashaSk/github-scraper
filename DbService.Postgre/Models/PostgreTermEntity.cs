using ClassScraper.DomainObjects.Github;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLayer.PostgreService.Models
{
    class PostgreTermEntity
    {
        [Key]
        public string ID { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        [Column("Type")]
        public TermType TermType { get; set; }

        public string PostgreFileEntityId { get; set; }
        public PostgreFileEntity PostgreFileEntity { get; set; }

        public PostgreTermEntity() { }
        public TermEntity ToDomain(bool initFile = true)
        {
            return new TermEntity()
            {
                ID = Guid.Parse(ID),
                Name = Name,
                TermType = TermType,
                FileEntity = initFile ? PostgreFileEntity.ToDomain() : null
            };
        }

        public PostgreTermEntity(TermEntity domainEntity)
        {
            ID = domainEntity.ID.ToString();
            Name = domainEntity.Name;
            TermType = domainEntity.TermType;
            PostgreFileEntityId = domainEntity.FileEntity.ID.ToString();
        }
    }
}
