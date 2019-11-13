using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Middleware.PortalApiExceptionHandler
{
    public static class ApiExceptionHandlerExtensions
    {
        /// <summary>
        /// Adds middleware that will handle exceptions for requests and return an object with the exception data.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.UseMiddleware<ApiExceptionHandlerMiddleware>();
        }

        /// <summary>
        /// Adds middleware that will handle exceptions for requests and return an object with the exception data.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, ApiExceptionHandlerOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            return app.UseMiddleware<ApiExceptionHandlerMiddleware>(Options.Create(options));
        }
    }
}
