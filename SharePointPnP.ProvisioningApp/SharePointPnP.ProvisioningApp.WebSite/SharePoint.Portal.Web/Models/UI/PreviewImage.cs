using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.UI
{
    public class PreviewImage
    {
        /// <summary>
        /// The type of the image (cardpreview or fullpage)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The url of the image
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The alt text to use for the image
        /// </summary>
        public string AltText { get; set; }
    }
}
