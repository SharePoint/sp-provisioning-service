using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning
{
    /// <summary>
    /// Defines a provisioning Action
    /// </summary>
    public class ProvisioningActionModel
    {
        /// <summary>
        /// Defines a unique ID for the action
        /// </summary>
        [JsonProperty(PropertyName = "correlationId")]
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// References the target Office 365 tenant
        /// </summary>
        [DisplayName("Tenant ID")]
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// References the user who is asking to execute the provisioning action
        /// </summary>
        [DisplayName("UserPrincipalName")]
        [JsonProperty(PropertyName = "upn")]
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Defines the email address to use for sending notifications
        /// </summary>
        [DisplayName("Notification Email")]
        [JsonProperty(PropertyName = "notificationEmail")]
        public string NotificationEmail { get; set; }

        /// <summary>
        /// Represents the Package to apply to the target
        /// </summary>
        [DisplayName("Package ID")]
        [JsonProperty(PropertyName = "packageId")]
        public String PackageId { get; set; }

        /// <summary>
        /// Defines any custom property for the provisioning of the Package
        /// </summary>
        [DisplayName("Package Properties")]
        [JsonProperty(PropertyName = "packageProperties")]
        public Dictionary<String, String> PackageProperties { get; set; }

        [DisplayName("Package Name")]
        public String DisplayName { get; set; }

        [DisplayName("Action Type")]
        [JsonProperty(PropertyName = "actionType")]
        public ActionType ActionType { get; set; }

        [DisplayName("Theme Name")]
        [JsonProperty(PropertyName = "themeName")]
        public String ThemeName { get; set; }

        [DisplayName("Primary Theme Color")]
        [JsonProperty(PropertyName = "themePrimaryColor")]
        public String ThemePrimaryColor { get; set; }

        [DisplayName("Body Text Color")]
        [JsonProperty(PropertyName = "themeBodyTextColor")]
        public String ThemeBodyTextColor { get; set; }

        [DisplayName("Body Background Color")]
        [JsonProperty(PropertyName = "themeBodyBackgroundColor")]
        public String ThemeBodyBackgroundColor { get; set; }

        [DisplayName("Custom Logo")]
        [JsonProperty(PropertyName = "customLogo")]
        public String CustomLogo { get; set; }

        [DisplayName("User is SPO Admin")]
        [JsonIgnore]
        public Boolean UserIsSPOAdmin { get; set; }

        [DisplayName("User is Tenant Global Admin")]
        [JsonIgnore]
        public Boolean UserIsTenantAdmin { get; set; }

        [JsonIgnore]
        public String ProvisionDescription { get; set; }

        [JsonIgnore]
        public Dictionary<String, MetadataProperty> MetadataProperties { get; set; }
    }

    /// <summary>
    /// Defines the flavors for ProvisioningActionModel instances
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// The action targets a Site Collection and, as such, any SPO user
        /// </summary>
        Site,
        /// <summary>
        /// The action targets the whole Tenant and, as such, only SPO administrators
        /// </summary>
        Tenant,
    }

    public class MetadataProperty
    {
        public String Name { get; set; }

        public String Caption { get; set; }

        public String Description { get; set; }
    }
}
