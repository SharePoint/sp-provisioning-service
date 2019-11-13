using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharePoint.Portal.Web.Models
{
    public class PageTemplate : BaseModel<string>
    {
        /// <summary>
        /// The html portion of the template
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// The css portion of the template
        /// </summary>
        public string Css { get; set; }
    }
}
