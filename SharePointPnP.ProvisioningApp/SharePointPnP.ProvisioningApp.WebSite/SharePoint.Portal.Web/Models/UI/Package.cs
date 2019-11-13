using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace SharePoint.Portal.Web.Models.UI
{
    public class Package
    {
        /// <summary>
        /// The Id of the package
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the package
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The abstract of the package
        /// </summary>
        public string Abstract { get; set; }

        /// <summary>
        /// The type of package (SiteCollection or Tenant)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType PackageType { get; set; }

        /// <summary>
        /// The deserialized display info used on the details page
        /// </summary>
        public DisplayInfo DisplayInfo { get; set; }

        /// <summary>
        /// The url for the provisioning form
        /// </summary>
        public string ProvisioningFormUrl { get; set; }
    }
}
