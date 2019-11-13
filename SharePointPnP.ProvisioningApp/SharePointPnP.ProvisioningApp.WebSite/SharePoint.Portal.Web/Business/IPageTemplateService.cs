using SharePoint.Portal.Web.Models;
using System;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business
{
    public interface IPageTemplateService
    {
        Task<PageTemplate> GetPageTemplate(string id);
    }
}
