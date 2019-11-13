using Microsoft.AspNetCore.Mvc;
using SharePoint.Portal.Web.Business;
using SharePoint.Portal.Web.QueryParams;
using System;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageTemplatesController : ControllerBase
    {
        private readonly IPageTemplateService pageTemplateService;

        public PageTemplatesController(IPageTemplateService pageTemplateService)
        {
            this.pageTemplateService = pageTemplateService;
        }

        [HttpGet]
        public async Task<ActionResult> GetById([FromQuery]PageTemplateQueryParams query)
        {
            var template = await pageTemplateService.GetPageTemplate(query.TemplateId);
            if (template == null)
            {
                return NotFound(new { Message = "Display template for page not found" });
            }
            return Ok(template);
        }
    }
}