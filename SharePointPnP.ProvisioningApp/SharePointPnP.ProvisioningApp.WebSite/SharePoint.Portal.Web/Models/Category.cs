using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Models
{
    public class Category : BaseModel<string>
    {
        /// <summary>
        /// The DisplayName of the Category
        /// </summary>
        [StringLength(200)]
        [Required]
        public String DisplayName { get; set; }

        /// <summary>
        /// Manual sort order field
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The Packages associated with the Category (many to many)
        /// </summary>
        public ICollection<PackageCategory> PackageCategories { get; set; }
    }
}
