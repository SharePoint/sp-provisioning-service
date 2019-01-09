using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// A First Release Office 365 Tenant
    /// </summary>
    public class Tenant : BaseModel<String>
    {
        /// <summary>
        /// The name of the First Release Tenant
        /// </summary>
        [StringLength(100)]
        [Required]
        public String TenantName { get; set; }

        /// <summary>
        /// The description/name of the reference owner
        /// </summary>
        [StringLength(200)]
        public String ReferenceOwner { get; set; }
    }
}
