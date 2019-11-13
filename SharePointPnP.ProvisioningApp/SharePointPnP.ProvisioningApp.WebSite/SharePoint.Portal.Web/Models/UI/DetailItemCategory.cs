using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.UI
{
    public class DetailItemCategory
    {
        /// <summary>
        /// The name of the category of items
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The items
        /// </summary>
        public List<DetailItem> Items { get; set; }
    }
}
