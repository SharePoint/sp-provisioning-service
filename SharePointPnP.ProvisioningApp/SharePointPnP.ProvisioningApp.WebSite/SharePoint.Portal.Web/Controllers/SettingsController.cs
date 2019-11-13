using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Business.DependencyInjection;
using System;

namespace SharePoint.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly GlobalOptions globalOptions;

        public SettingsController(IOptionsMonitor<GlobalOptions> globalOptions)
        {
            this.globalOptions = globalOptions.CurrentValue;
        }

        public ActionResult Get()
        {
            return Ok(new
            {
                ServerDateTime = DateTime.UtcNow,
                globalOptions.ProvisioningPageBaseUrl,
                TargetPlatformId = globalOptions.PlatformId
            });
        }
    }
}