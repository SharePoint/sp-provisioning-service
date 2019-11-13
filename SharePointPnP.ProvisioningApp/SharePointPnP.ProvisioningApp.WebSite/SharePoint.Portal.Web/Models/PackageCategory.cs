using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models
{
    public class PackageCategory
    {
        public Guid PackageId { get; set; }

        public string CategoryId { get; set; }

        [ForeignKey("PackageId")]
        public Package Package { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
