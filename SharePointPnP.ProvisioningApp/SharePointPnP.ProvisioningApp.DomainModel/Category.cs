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
    /// The Category for a Package
    /// </summary>
    public class Category : BaseModel<String>
    {
        /// <summary>
        /// The DisplayName of the Category
        /// </summary>
        [StringLength(200)]
        [Required]
        public String DisplayName { get; set; }

        /// <summary>
        /// Defines the ordinal position of the category
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The Packages associated with the Category (many to many)
        /// </summary>
        public List<Package> Packages { get; set; }
    }
}
