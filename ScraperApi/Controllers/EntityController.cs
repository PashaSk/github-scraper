using ClassScraper.DomainObjects.Github;
using ClassScraper.Repository.EntityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScraperApi.Helpers;
using ScraperApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ScraperApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public class EntityController : ControllerBase
    {
        private IEntityRepository Repository { get; }
        private ILogger<EntityController> Logger { get; }
        public EntityController(
            IEntityRepository repository,
            ILogger<EntityController> logger
            )
        {
            Repository = repository;
            Logger = logger;
        }
        
        [HttpGet]
        [LinkHeader]
        public async Task<ActionResult<ListSearchResult<TermEntity>>> Terms([FromQuery] ApiSearchModel filter, CancellationToken ctoken)
        {
            Logger.LogInformation("FILTER: {value}", filter);
            var terms = await Repository.GetTermsAsync(filter.ToDomain(), ctoken);
            return Ok(terms);
        }

        [HttpGet]
        [LinkHeader]
        public async Task<ActionResult<ListSearchResult<FileEntity>>> Files([FromQuery] ApiSearchModel filter, CancellationToken ctoken)
        {
            Logger.LogInformation("FILTER: {value}", filter);
            var files = await Repository.GetFilesAsync(filter.ToDomain(), ctoken);

            return Ok(files);
        }

        [HttpGet]
        public async Task<ActionResult<FileEntity>> File([FromQuery] ApiSearchModel filter, CancellationToken ctoken)
        {
            Logger.LogInformation("FILTER: {value}", filter);
            var files = await Repository.GetFileAsync(filter.ToDomain(), ctoken);

            return Ok(files);
        }

        [HttpGet]
        public async Task<ActionResult<TermEntity>> Term([FromQuery] ApiSearchModel filter, CancellationToken ctoken)
        {
            Logger.LogInformation("FILTER: {value}", filter);
            var files = await Repository.GetTermAsync(filter.ToDomain(), ctoken);

            return Ok(files);
        }
    }
}
