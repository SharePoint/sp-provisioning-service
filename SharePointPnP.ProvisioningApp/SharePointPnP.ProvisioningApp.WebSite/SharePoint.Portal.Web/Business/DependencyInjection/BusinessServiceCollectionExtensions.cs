using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharePoint.Portal.Web.Business.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up common web services.
    /// </summary>
    public static class BusinessServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default services and filters to the service collection. Provide your own services
        /// to override any of these by registering them before calling this method.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.TryAddScoped<IPackageService, PackageService>();
            services.TryAddScoped<ICategoryService, CategoryService>();
            services.TryAddScoped<IPageTemplateService, PageTemplateService>();

            return services;
        }
    }
}
