using ClassScraper.DomainObjects.Github;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ScraperApi.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace ScraperApi.Helpers
{
    public class LinkHeaderAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is OkObjectResult)
            {
                var result = ((OkObjectResult)context.Result).Value;
                if (result is IListSearchResult)
                {
                    var totalCount = ((IListSearchResult)result).TotalCount;
                    var baseQuery = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.ToString());
                    int page = 1;
                    if (baseQuery[ApiSearchModel.PAGE_QUERY] != null)
                    {
                        if (int.TryParse(baseQuery[ApiSearchModel.PAGE_QUERY], out int parseInt))
                        {
                            page = parseInt;
                        }
                    }

                    AppendLinkHeaders(context.HttpContext, totalCount, page, baseQuery);
                }

            }
            base.OnResultExecuting(context);
        }

        private void AppendLinkHeaders(HttpContext context, int totalCount, int page, NameValueCollection baseQuery)
        {
            var links = new Dictionary<string, string>();
            var uri = new Uri(context.Request.GetEncodedUrl());
            var builder = new UriBuilder(uri);

            //rel='first'
            baseQuery.Set(ApiSearchModel.PAGE_QUERY, "1");
            builder.Query = baseQuery.ToString();
            var uriString = builder.Uri.AbsoluteUri;
            links.Add("first", uriString);

            //rel='prev'
            if (page > 1)
            {
                baseQuery.Set(ApiSearchModel.PAGE_QUERY, (page - 1).ToString());
                builder.Query = baseQuery.ToString();
                var prev = builder.Uri.AbsoluteUri;
                links.Add("prev", prev);
            }

            //rel='next'
            var totalPages = Math.Ceiling((double)totalCount / 5);
            if (page < totalPages)
            {
                baseQuery.Set(ApiSearchModel.PAGE_QUERY, (page + 1).ToString());
                builder.Query = baseQuery.ToString();
                var prev = builder.Uri.AbsoluteUri;
                links.Add("next", prev);
            }

            //rel='last'
            if (totalPages > 0)
            {
                baseQuery.Set(ApiSearchModel.PAGE_QUERY, totalPages.ToString());
                builder.Query = baseQuery.ToString();
                var prev = builder.Uri.AbsoluteUri;
                links.Add("last", prev);
            }


            var header = links.Select(k => $"<{k.Value}>; rel='{k.Key}'");
            context.Response.Headers.Add("Link", string.Join(',', header));
        }
    }
}
