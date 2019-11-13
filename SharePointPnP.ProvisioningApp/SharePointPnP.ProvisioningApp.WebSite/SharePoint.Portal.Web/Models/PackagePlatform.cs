using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models
{
    public class PackagePlatform
    {
        public string PlatformId { get; set; }

        public Guid PackageId { get; set; }

        public Platform Platform { get; set; }

        public Package Package { get; set; }
    }
}
