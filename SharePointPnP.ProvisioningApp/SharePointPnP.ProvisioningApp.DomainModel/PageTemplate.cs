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
    /// Defines a Page Template for rendering a package
    /// </summary>
    public class PageTemplate : BaseModel<String>
    {
        /// <summary>
        /// The relative path of the HTML file 
        /// </summary>
        [Required]
        public String Html { get; set; }

        /// <summary>
        /// The relative path of the CSS file 
        /// </summary>
        [Required]
        public String Css { get; set; }
    }
}
