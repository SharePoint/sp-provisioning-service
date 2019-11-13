using System.ComponentModel.DataAnnotations;

namespace SharePoint.Portal.Web.Business.DependencyInjection
{
    public class GlobalOptions
    {
        [Required]
        public string ProvisioningPageBaseUrl { get; set; }

        public bool IsTestEnvironment { get; set; }

        public string PlatformId { get; set; }
    }
}
