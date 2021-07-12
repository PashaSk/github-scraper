using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScraper.DomainObjects.Github
{
    public interface IDomainEntity
    {
        public Guid ID { get; set; }
    }
}
