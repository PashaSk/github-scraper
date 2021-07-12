using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ClassScraper.DomainObjects.Github
{
    [JsonObject]
    public class TermEntity : IDomainEntity
    {        
        [JsonProperty("id")]
        public Guid ID { get;  set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [Column("Type")]
        [JsonProperty("term_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TermType TermType { get; set; }
        [JsonProperty("file")]
        public FileEntity FileEntity { get; set; }
    }

    public enum TermType
    {
        Undefined = 0,
        Class = 1,
        Interface = 2,
        Enum = 3,
        Property = 4,
        Field = 5
    }
}
