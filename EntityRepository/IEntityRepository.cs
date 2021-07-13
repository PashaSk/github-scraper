using ClassScraper.DomainObjects.API;
using ClassScraper.DomainObjects.Github;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassScraper.Repository.EntityService
{
    public interface IEntityRepository
    {
        public Task SaveEntitiesBatch(IEnumerable<TermEntity> terms, FileEntity file, CancellationToken ctoken = default);        
        public Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel filter, CancellationToken ctoken = default);
        public Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel filter, CancellationToken ctoken = default);
        public Task<FileEntity> GetFileAsync(SearchModel filter, CancellationToken ctoken = default);
        public Task<TermEntity> GetTermAsync(SearchModel filter, CancellationToken ctoken= default);
    }
}
