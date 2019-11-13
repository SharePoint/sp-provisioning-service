using System.Collections.Generic;

namespace SharePoint.Portal.Web.Models.UI
{
    // TODO: maybe we want a different name for this
    public class Category
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public List<Package> Packages { get; set; }
    }
}
