using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScraper.DomainObjects.Github
{    
    public class ListSearchResult<T> : IListSearchResult
    {
        public IEnumerable<T> Data { get; set; }        
        public int TotalCount { get; set; }
    }
}
