//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// A Notification Recipient
    /// </summary>
    public class NotificationRecipient: BaseModel<Guid>
    {
        /// <summary>
        /// The DisplayName of the Notification Recipient
        /// </summary>
        [StringLength(200)]
        [Required]
        public String DisplayName { get; set; }

        /// <summary>
        /// The Email address of the Notification Recipient
        /// </summary>
        [StringLength(200)]
        [Required]
        public String Email { get; set; }

        /// <summary>
        /// Defines whether the Notification Recipient is enabled or not
        /// </summary>
        public bool Enabled { get; set; }
    }
}
