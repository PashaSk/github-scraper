using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClassScraper.DomainObjects.Github
{
    [JsonObject]
    public class FileEntity : IDomainEntity
    {
        [JsonProperty("id")]
        public Guid ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        [JsonProperty("owner")]
        public string OwnerName { get; set; }
        [JsonProperty("repository")]
        public string RepositoryName { get; set; }
        [JsonProperty("terms", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<TermEntity> Terms { get; set; }
    }
}
