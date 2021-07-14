using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ScraperApi.Helpers
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 204,
                    Content = "Operation was canceled"
                };
            }
        }
    }
}
