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
        public Task SaveEntitiesBatch(IEnumerable<TermEntity> terms, FileEntity file, CancellationToken cancellationToken);        
        public Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel filter);
        public Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel filter);
    }
}
