using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.UI
{
    public class DetailItem
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional URL that the item should link to
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Optional badge text to display on the item
        /// </summary>
        public string BadgeText { get; set; }

        /// <summary>
        /// Optional url of image to display for the item
        /// </summary>
        public string PreviewImage { get; set; }
    }
}
