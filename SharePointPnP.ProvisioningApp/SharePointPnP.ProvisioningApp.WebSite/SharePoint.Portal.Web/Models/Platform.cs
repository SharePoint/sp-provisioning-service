using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models
{
    public class Platform : BaseModel<string>
    {
        /// <summary>
        /// Display name of the platform
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Relationship table with the related packages
        /// </summary>
        public List<PackagePlatform> PackagePlatforms {get; set;}
    }
}
