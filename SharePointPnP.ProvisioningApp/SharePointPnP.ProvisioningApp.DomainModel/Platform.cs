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
    public class Platform : BaseModel<String>
    {
        /// <summary>
        /// The DisplayName of the Platform
        /// </summary>
        [StringLength(200)]
        [Required]
        public String DisplayName { get; set; }

        /// <summary>
        /// The Packages associated with the Platform (many to many)
        /// </summary>
        public List<Package> Packages { get; set; }
    }
}
