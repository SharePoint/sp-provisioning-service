using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// The Content for a Page in the site
    /// </summary>
    public class ContentPage : BaseModel<String>
    {
        /// <summary>
        /// The content of a Page
        /// </summary>
        [Required]
        public String Content { get; set; }
    }
}
