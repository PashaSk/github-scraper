using ClassScraper.DomainObjects.Github;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScraper.DbLayer.MongoDb.Models
{
    public interface IMongoEntity<D>
    {
        public D ToDomain();
    }
}
