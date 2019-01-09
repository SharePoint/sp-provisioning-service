using System.Web;
using System.Web.Mvc;

namespace SharePointPnP.ProvisioningApp.WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
