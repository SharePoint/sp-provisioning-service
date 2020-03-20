//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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
        [DisplayName("Email")]
        [JsonProperty(PropertyName = "notificationEmail")]
        public string NotificationEmail { get; set; }

        /// <summary>
        /// Represents the Package ID to apply to the target
        /// </summary>
        [DisplayName("Package ID")]
        [JsonProperty(PropertyName = "packageId")]
        public String PackageId { get; set; }

        /// <summary>
        /// Provides the URL of Image Preview for the Package to apply to the target
        /// </summary>
        [DisplayName("PackageImagePreviewUrl")]
        [JsonIgnore]
        public String PackageImagePreviewUrl { get; set; }

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

        // [DisplayName("Apply theme")]
        [JsonProperty(PropertyName = "applyTheme")]
        public Boolean ApplyTheme { get; set; } = false;

        [JsonIgnore]
        public List<String> Themes { get; set; }

        [DisplayName("Theme")]
        [JsonProperty(PropertyName = "selectedTheme")]
        public String SelectedTheme { get; set; }

        // [DisplayName("Apply custom theme")]
        [JsonProperty(PropertyName = "applyCustomTheme")]
        public Boolean ApplyCustomTheme { get; set; } = false;

        [DisplayName("Theme Name")]
        [JsonProperty(PropertyName = "themeName")]
        public String ThemeName { get; set; }

        [DisplayName("Primary Theme Color")]
        [JsonProperty(PropertyName = "themePrimaryColor")]
        public String ThemePrimaryColor { get; set; } = "#03787C";

        [DisplayName("Body Text Color")]
        [JsonProperty(PropertyName = "themeBodyTextColor")]
        public String ThemeBodyTextColor { get; set; } = "#000000";

        [DisplayName("Body Background Color")]
        [JsonProperty(PropertyName = "themeBodyBackgroundColor")]
        public String ThemeBodyBackgroundColor { get; set; } = "#FFFFFF";

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

        [AllowHtml]
        [JsonIgnore]
        public String MetadataPropertiesJson { get; set; }

        [JsonIgnore]
        public String Instructions { get; set; }

        [JsonIgnore]
        public String ProvisionRecap { get; set; }

        [JsonIgnore]
        public String SPORootSiteUrl { get; set; }

        public Boolean TargetSiteAlreadyExists { get; set; } = false;

        public String TargetSiteBaseTemplateId { get; set; }

        [JsonIgnore]
        public String MissingPreReqsHeader { get; set; }

        [JsonIgnore]
        public String MissingPreReqsFooter { get; set; }

        [JsonIgnore]
        public String MatchingSiteBaseTemplateId { get; set; }

        [JsonIgnore]
        public Boolean ForceNewSite { get; set; }

        public List<ProvisioningItemModel> ChildrenItems { get; set; }

        public List<ProvisioningWebhook> Webhooks { get; set; }

        [JsonIgnore]
        public String ReturnUrl { get; set; }

        [JsonIgnore]
        public List<ProvisioningPreRequirement> ProvisioningPreRequirements { get; set; }

        [JsonIgnore]
        public List<String> PreRequirementIssues { get; set; }
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

    /// <summary>
    /// Defines a metadata property for the template/package
    /// </summary>
    public class MetadataProperty
    {
        /// <summary>
        /// The name of the metadata property
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The caption of the metadata property
        /// </summary>
        public String Caption { get; set; }

        /// <summary>
        /// The description of the metadata property
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// The editor (CSHTML control) for the metadata property
        /// </summary>
        public String Editor { get; set; }

        /// <summary>
        /// The custom settings for the Editor
        /// </summary>
        public String EditorSettings { get; set; }
    }
}
