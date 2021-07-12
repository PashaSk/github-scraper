using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScraper.DomainObjects.Github
{
    public interface IListSearchResult
    {
        public int TotalCount { get; set; }
    }
}
