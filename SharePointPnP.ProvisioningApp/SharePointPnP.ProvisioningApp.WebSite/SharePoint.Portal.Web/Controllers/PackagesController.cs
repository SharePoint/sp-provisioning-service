using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Business;
using SharePoint.Portal.Web.Business.DependencyInjection;
using SharePoint.Portal.Web.Models.UI;
using SharePoint.Portal.Web.QueryParams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    public class PackagesController : Controller
    {
        private readonly GlobalOptions globalOptions;
        private readonly IPackageService packageService;

        public PackagesController(IOptionsMonitor<GlobalOptions> globalOptions, IPackageService packageService)
        {
            this.globalOptions = globalOptions.CurrentValue;
            this.packageService = packageService;
        }

        [HttpGet]
        public async Task<IEnumerable<Package>> Get([FromQuery]PackageQueryParams query)
        {
            return await packageService.GetFromQuery(doIncludePreview: globalOptions.IsTestEnvironment, categoryId: query.CategoryId);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult> Get(Guid id)
        {
            var package = await packageService.GetById(id, doIncludePreview: globalOptions.IsTestEnvironment);
            if (package == null)
            {
                return NotFound(new { Message = "Package not found" });
            }
            return Ok(package);
        }
    }
}
