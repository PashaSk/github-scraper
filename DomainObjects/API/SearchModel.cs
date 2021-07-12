using System;

namespace ClassScraper.DomainObjects.API
{    
    public struct SearchModel
    {        
        public string FilterName { get;  set; }     
        public string FilterId { get; set; }
        public SearchGroupBy GroupBy { get; set; }
        public int Page { get; set; }
    }

    [Flags]
    public enum SearchGroupBy
    {
        None= 0,
        Type = 1,
        Name = 2
    }
}
