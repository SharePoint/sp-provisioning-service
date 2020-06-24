using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// Defines the settings for a template (models the settigs.json file content)
    /// </summary>
    public class TemplateSettings
    {
        public string templateId { get; set; }

        public string @abstract { get; set; }

        public int sortOrder { get; set; }

        public String[] categories { get; set; }

        public string packageFile { get; set; }

        public bool promoted { get; set; }

        public int sortOrderPromoted { get; set; }

        public bool preview { get; set; }

        public string matchingSiteBaseTemplateId { get; set; }

        public bool forceNewSite { get; set; }

        public bool visible { get; set; }

        public TemplateSettingsMetadata metadata { get; set; }

        public String[] platforms { get; set; }
    }

    public class TemplateSettingsMetadata
    {
        public TemplateSettingsMetadataProperties[] properties { get; set; }

        public TemplateSettingsMetadataDisplayInfo displayInfo { get; set; }
    }

    public class TemplateSettingsMetadataProperties
    {
        public string name { get; set; }

        public string caption { get; set; }

        public string description { get; set; }

        public string editor { get; set; }

        public string editorSettings { get; set; }
    }

    public class TemplateSettingsMetadataDisplayInfo
    {
        public string pageTemplateId { get; set; }

        public string siteDescriptor { get; set; }

        public string[] descriptionParagraphs { get; set; }

        public TemplateSettingsMetadataDisplayInfoPreviewImage[] previewImages { get; set; }

        public TemplateSettingsMetadataDisplayInfoDetailItemCategory[] detailItemCategories { get; set; }

        public TemplateSettingsMetadataDisplayInfoSystemRequirement[] systemRequirements { get; set; }
    }

    public class TemplateSettingsMetadataDisplayInfoPreviewImage
    {
        public string type { get; set; }

        public string altText { get; set; }

        public string url { get; set; }
    }

    public class TemplateSettingsMetadataDisplayInfoDetailItemCategory
    {
        public string name { get; set; }

        public TemplateSettingsMetadataDisplayInfoDetailItemCategoryItem[] items { get; set; }
    }

    public class TemplateSettingsMetadataDisplayInfoDetailItemCategoryItem
    {
        public string name { get; set; }

        public string description { get; set; }

        public string url { get; set; }

        public string badgeText { get; set; }

        public string previewImage { get; set; }
    }

    public class TemplateSettingsMetadataDisplayInfoSystemRequirement
    {
        public string name { get; set; }

        public string value { get; set; }
    }
}
