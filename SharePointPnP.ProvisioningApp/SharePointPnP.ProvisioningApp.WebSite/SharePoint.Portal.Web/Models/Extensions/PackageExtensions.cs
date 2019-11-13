using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.Extensions
{
    public static class PackageExtensions
    {
        public static UI.Package ToUiFormat(this Package package, string provisioningPageBaseUrl, bool doIncludeDisplayInfo)
        {
            var formUrl = new UriBuilder(new Uri(provisioningPageBaseUrl));

            if (!formUrl.Path.EndsWith("/"))
            {
                formUrl.Path += "/";
            }
            if (package.PackageType == PackageType.SiteCollection)
            {
                formUrl.Path += "site";
            }
            if (package.PackageType == PackageType.Tenant)
            {
                formUrl.Path += "tenant";
            }
            formUrl.Path += "/home/provision";

            formUrl.Query = $"packageId={package.Id}";

            return new UI.Package
            {
                Abstract = package.Abstract,
                DisplayInfo = (!doIncludeDisplayInfo || package.PropertiesMetadata == null) ? null : package.PropertiesMetadata.DisplayInfo,
                DisplayName = package.DisplayName,
                Id = package.Id,
                PackageType = package.PackageType,
                ProvisioningFormUrl = formUrl.Uri.ToString()
            };
        }
    }
}
