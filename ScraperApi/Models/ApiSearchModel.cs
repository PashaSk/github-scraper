using ClassScraper.DomainObjects.API;
using ClassScraper.DomainObjects.Github;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScraperApi.Models
{
    public class ApiSearchModel
    {
        public const string PAGE_QUERY = "page";

        [FromQuery(Name = "name")]
        public string FilterName { get; set; }
        [FromQuery(Name = "id")]
        public string FilterId { get; set; }
        [FromQuery(Name = "group_by")]
        public SearchGroupBy GroupBy { get; set; }
        [FromQuery(Name = PAGE_QUERY)]
        public int Page { get; set; } = 1;
        [FromQuery(Name = "term_type")]
        public TermType TermType { get; set; }
        [FromQuery(Name = "file_id")]
        public string FilterFileId { get; set; }
        
        public SearchModel ToDomain()
        {
            return new SearchModel()
            {
                FilterName = FilterName,
                FilterId = FilterId,
                GroupBy = GroupBy,
                Page = Page == 0 ? 0 : (Page - 1),
                TermType = TermType,
                FilterFileId = FilterFileId
            };
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
