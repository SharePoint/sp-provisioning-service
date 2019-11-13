using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.QueryParams
{
    public class CategoryQueryParams
    {
        /// <summary>
        /// Set to true in packages should be included in the return
        /// </summary>
        public bool DoIncludeDisplayInfo { get; set; } = false;
    }
}
