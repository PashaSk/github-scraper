using ClassScraper.DomainObjects.API;
using ClassScraper.DomainObjects.Github;
using ClassScraper.Repository.EntityService;
using Microsoft.Extensions.Logging;
using Moq;
using ScraperApi.Controllers;
using ScraperApi.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ScraperApi.Tests
{
    public class EntityControllerTest
    {
        [Fact]
        public async Task LinkHeader()
        {
            var repositoryMock = new Mock<IEntityRepository>();
            repositoryMock.Setup(m => m.GetFilesAsync(It.IsAny<SearchModel>(), CancellationToken.None)).Returns(Task.FromResult(new ListSearchResult<FileEntity>() { Data = null, TotalCount = 100 }));
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
            });
            var logger = loggerFactory.CreateLogger<EntityController>();
            var filter = new ApiSearchModel()
            {
                Page = 1
            };
            var controller = new EntityController(repositoryMock.Object, logger);
            var result = await controller.Files(filter, CancellationToken.None);
            logger.LogInformation(result.ToString());
        }
    }
}
