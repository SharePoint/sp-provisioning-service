//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
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

        public TemplateSettingsPreRequirement[] preRequirements { get; set; }
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

        public string siteTitle { get; set; }

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

    /// <summary>
    /// Defines a pre-requirements
    /// </summary>
    public class TemplateSettingsPreRequirement
    {
        /// <summary>
        /// The name of the assembly for the custom pre-check component
        /// </summary>
        public string assemblyName { get; set; }

        /// <summary>
        /// The name of the type for the custom pre-check component
        /// </summary>
        public string typeName { get; set; }

        /// <summary>
        /// The configuration for the custom pre-check component
        /// </summary>
        public string configuration { get; set; }

        /// <summary>
        /// The id of the document with the pre-requirement description
        /// </summary>
        public string preRequirementContent { get; set; }
    }
}
