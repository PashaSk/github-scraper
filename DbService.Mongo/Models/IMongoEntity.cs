namespace ClassScraper.DbLayer.MongoDb.Models
{
    public interface IMongoEntity<D>
    {
        public D ToDomain();
    }
}
