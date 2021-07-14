using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using System;
using System.Web;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Net.Http;
using System.Net.Http.Headers;
using ScraperApi.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;

namespace ScraperApi.Tests
{
    public class LinkHeaderFilterTest
    {
        private ILogger<LinkHeaderFilterTest> _logger;
        private LinkHeaderAttribute _filter;
        private NameValueCollection _baseQuery;
        public LinkHeaderFilterTest()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<LinkHeaderFilterTest>();
            _filter = new LinkHeaderAttribute();
            _baseQuery = HttpUtility.ParseQueryString("?name=searchName");
        }        
        private const string TEST_URI = "https://my-domain.com/api/scraper";
        
        [Fact]
        public void CheckPaginationFirstPage()
        {            
            var links = _filter.BuildLinkHeaders(TEST_URI, 15, 1, _baseQuery);
            Assert.Equal(s_simpleQueryExpected, links);            
        }

        [Fact]
        public void CheckPaginationLastPage()
        {
            var links = _filter.BuildLinkHeaders(TEST_URI, 15, 3, _baseQuery);
            Assert.Equal(s_simpleQueryFirstPageExpected, links);
        }

        [Fact]
        public void CheckPaginationMiddlePage()
        {
            var links = _filter.BuildLinkHeaders(TEST_URI, 34, 3, _baseQuery);
            Assert.Equal(s_simplQueryMiddlePageExpected, links);
        }


        #region theory_data
        private static IEnumerable<string> s_simpleQueryExpected = new List<string>()
        {
            "<https://my-domain.com/api/scraper?name=searchName&page=1>; rel='first'",
            "<https://my-domain.com/api/scraper?name=searchName&page=2>; rel='next'",
            "<https://my-domain.com/api/scraper?name=searchName&page=3>; rel='last'",
        };
        private static IEnumerable<string> s_simpleQueryFirstPageExpected = new List<string>()
        {
            "<https://my-domain.com/api/scraper?name=searchName&page=1>; rel='first'",            
            "<https://my-domain.com/api/scraper?name=searchName&page=2>; rel='prev'",            
            "<https://my-domain.com/api/scraper?name=searchName&page=3>; rel='last'",
        };
        private static IEnumerable<string> s_simplQueryMiddlePageExpected = new List<string>()
        {
            "<https://my-domain.com/api/scraper?name=searchName&page=1>; rel='first'",
            "<https://my-domain.com/api/scraper?name=searchName&page=2>; rel='prev'",
            "<https://my-domain.com/api/scraper?name=searchName&page=4>; rel='next'",
            "<https://my-domain.com/api/scraper?name=searchName&page=7>; rel='last'",
        };
        #endregion
    }
}
