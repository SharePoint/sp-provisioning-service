using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// Defines a Provisioning action which is running within the job
    /// </summary>
    public class ProvisioningActionItem : BaseModel<Guid>
    {
        /// <summary>
        /// References the target Office 365 tenant
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Represents the Package to apply to the target
        /// </summary>
        public Guid PackageId { get; set; }

        /// <summary>
        /// JSON representation of the package properties
        /// </summary>
        public String PackageProperties { get; set; }

        /// <summary>
        /// Date and time of creation of the item
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Date and time of expiration
        /// </summary>
        public DateTime ExpiresOn { get; set; }
    }
}
