using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.UI
{
    public class DisplayInfo
    {
        /// <summary>
        /// The Id of the page template to use to display the details
        /// </summary>
        public string PageTemplateId { get; set; }

        /// <summary>
        /// Site descriptor
        /// </summary>
        public string SiteDescriptor { get; set; }

        /// <summary>
        /// The paragraphs that make up the description text
        /// </summary>
        public List<string> DescriptionParagraphs { get; set; }

        /// <summary>
        /// The images for the package. This is for the small card and for the details page.
        /// </summary>
        public List<PreviewImage> PreviewImages { get; set; }

        /// <summary>
        /// The categorized items for the details
        /// </summary>
        public List<DetailItemCategory> DetailItemCategories { get; set; }

        /// <summary>
        /// The requirements for the package
        /// </summary>
        public List<SystemRequirement> SystemRequirements { get; set; }
    }
}
