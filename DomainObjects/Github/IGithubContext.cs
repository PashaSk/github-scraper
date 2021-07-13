using ClassScraper.DomainObjects.API;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassScraper.DomainObjects.Github
{
    public interface IGithubContext
    {
        Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel search, CancellationToken ctoken);
        Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel search, CancellationToken ctoken);
        Task<FileEntity> GetFileAsync(SearchModel search, CancellationToken ctoken);
        Task<TermEntity> GetTermAsync(SearchModel search, CancellationToken ctoken);
        Task<bool> SaveTermsAsync(FileEntity file, IEnumerable<TermEntity> terms, CancellationToken ctoken);        
    }
}
    