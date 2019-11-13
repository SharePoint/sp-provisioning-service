using SharePoint.Portal.Web.Models.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get all categories, including the packages in the categories.
        /// </summary>
        /// <param name="doIncludePreview">Set to true to include preview packages</param>
        /// <param name="doIncludeDisplayInfo">Set to true to include the display info</param>
        /// <returns></returns>
        Task<List<Category>> GetAllWithPackages(bool doIncludePreview = false, bool doIncludeDisplayInfo = false);
    }
}
