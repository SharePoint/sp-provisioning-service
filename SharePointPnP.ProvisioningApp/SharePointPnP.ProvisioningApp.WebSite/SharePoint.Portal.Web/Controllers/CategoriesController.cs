using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Business;
using SharePoint.Portal.Web.Business.DependencyInjection;
using SharePoint.Portal.Web.Models.UI;
using SharePoint.Portal.Web.QueryParams;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesController
    {
        private readonly GlobalOptions globalOptions;
        private readonly ICategoryService categoryService;

        public CategoriesController(IOptionsMonitor<GlobalOptions> globalOptions, ICategoryService categoryService)
        {
            this.globalOptions = globalOptions.CurrentValue;
            this.categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IEnumerable<Category>> GetAll([FromQuery] CategoryQueryParams query)
        {
            return await categoryService.GetAllWithPackages(doIncludePreview: globalOptions.IsTestEnvironment, doIncludeDisplayInfo: query.DoIncludeDisplayInfo);
        }
    }
}
