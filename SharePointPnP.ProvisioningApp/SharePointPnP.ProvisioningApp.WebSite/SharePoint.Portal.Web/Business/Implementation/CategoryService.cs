using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Business.DependencyInjection;
using SharePoint.Portal.Web.Data;
using SharePoint.Portal.Web.Models.Extensions;
using SharePoint.Portal.Web.Models.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business.Implementation
{
    public class CategoryService : ICategoryService
    {
        public PortalDbContext DbContext { get; set; }

        private GlobalOptions GlobalOptions { get; set; }

        public CategoryService(PortalDbContext dbContext, IOptionsMonitor<GlobalOptions> globalOptions)
        {
            DbContext = dbContext;
            GlobalOptions = globalOptions.CurrentValue;
        }

        public Task<List<Category>> GetAllWithPackages(bool doIncludePreview = false, bool doIncludeDisplayInfo = false)
        {
            return DbContext.Categories
                .AsNoTracking()
                .Where(c => c.PackageCategories.Any(
                    pc => pc.Package.Visible &&
                    (doIncludePreview || !pc.Package.Preview) &&
                    pc.Package.PackagePlatforms.Any(pp => pp.PlatformId == GlobalOptions.PlatformId)
                ))
                .OrderBy(c => c.Order)
                .Select(c => new Category
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName,
                    Packages = c.PackageCategories
                        .Where(pc => pc.Package.Visible &&
                            (doIncludePreview || !pc.Package.Preview) &&
                            pc.Package.PackagePlatforms.Any(pp => pp.PlatformId == GlobalOptions.PlatformId)
                        )
                        .OrderBy(pc => pc.Package.SortOrder)
                        .Select(pc => pc.Package.ToUiFormat(GlobalOptions.ProvisioningPageBaseUrl, doIncludeDisplayInfo))
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
