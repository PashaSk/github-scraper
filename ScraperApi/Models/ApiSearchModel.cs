using ClassScraper.DomainObjects.API;
using Microsoft.AspNetCore.Mvc;
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

        public SearchModel ToDomain()
        {
            return new SearchModel()
            {
                FilterName = FilterName,
                FilterId = FilterId,
                GroupBy = GroupBy,
                Page = Page == 0 ? 0 : (Page - 1)
            };
        }
    }
}
