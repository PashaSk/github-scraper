using System.Collections.Generic;

namespace ClassScraper.DomainObjects.Github
{
    public class ListSearchResult<T> : IListSearchResult
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; }
    }
}
