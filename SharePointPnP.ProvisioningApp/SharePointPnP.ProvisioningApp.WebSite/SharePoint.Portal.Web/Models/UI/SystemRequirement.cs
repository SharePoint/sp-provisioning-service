using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models.UI
{
    public class SystemRequirement
    {
        /// <summary>
        /// The name of the requirement
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value that is needed to meet the requirement
        /// </summary>
        public string Value { get; set; }
    }
}
