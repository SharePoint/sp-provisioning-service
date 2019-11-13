using SharePoint.Portal.Web.Models.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business
{
    public interface IPackageService
    {
        /// <summary>
        /// Get template card by Id
        /// </summary>
        /// <param name="id">the template card Id</param>
        /// <returns>Template card with the provided Id</returns>
        Task<Package> GetById(Guid id, bool doIncludePreview = false);

        /// <summary>
        /// Gets template cards ordered by DisplayName
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Package>> GetFromQuery(bool doIncludePreview = false, string categoryId = null);
    }
}
