using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Middleware.PortalApiExceptionHandler
{
    public class ApiExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly ApiExceptionHandlerOptions options;

        private bool doIncludeDebugInfo;

        public ApiExceptionHandlerMiddleware(
            RequestDelegate next,
            IOptions<ApiExceptionHandlerOptions> options,
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options?.Value ?? throw new ArgumentException(nameof(options));
            logger = loggerFactory.CreateLogger<ApiExceptionHandlerMiddleware>();

            // If displaying the stack trace option has not been provided, default to true if development, false otherwise
            doIncludeDebugInfo = this.options.IncludeDebugInfo.HasValue ?
                this.options.IncludeDebugInfo.Value :
                hostingEnvironment.IsDevelopment();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments(options.PathStart))
            {
                await next(context);
                return;
            }

            try
            {
                await next(context);
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "An uncaught exception has occured while executing the request.");

                if (context.Response.HasStarted)
                {
                    logger.LogWarning("The response has already started, error handler will not execute.");
                    throw;
                }

                try
                {
                    if (e is PermissionDeniedException permException)
                    {
                        await DisplayPortalException(context, HttpStatusCode.Forbidden, permException.Message, "PermissionDenied", permException);
                        return;
                    }

                    if (e is PortalApiException portalException)
                    {
                        await DisplayPortalException(context, portalException.HttpStatusCode, portalException.Message, portalException.ApiErrorCode, portalException);
                        return;
                    }

                    await DisplayPortalException(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", "UnexpectedError", e);
                    return;
                }
                catch (Exception e2)
                {
                    logger.LogError(e2, "There was an error generating the error message.");
                }

                throw;
            }
        }

        private Task DisplayPortalException(HttpContext context, HttpStatusCode statusCode, string message, string apiErrorCode, Exception exception)
        {
            ObjectResult result;

            if (doIncludeDebugInfo)
            {
                result = new ObjectResult(new DebugErrorReturn
                {
                    Message = message,
                    ApiErrorCode = apiErrorCode,
                    Exception = exception
                });
            }
            else
            {
                result = new ObjectResult(new ErrorReturn
                {
                    Message = message,
                    ApiErrorCode = apiErrorCode
                });
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            var routeData = context.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(context, routeData, new ActionDescriptor());
            var executor = context.RequestServices.GetService<IActionResultExecutor<ObjectResult>>();

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
