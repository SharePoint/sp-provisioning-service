using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharePoint.Portal.Web.Business.DependencyInjection;
using SharePoint.Portal.Web.Data;
using SharePoint.Portal.Web.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Business.Implementation
{
    public class PackageService : IPackageService
    {
        public PortalDbContext DbContext { get; set; }

        private GlobalOptions GlobalOptions { get; set; }

        public PackageService(PortalDbContext dbContext, IOptionsMonitor<GlobalOptions> globalOptions)
        {
            DbContext = dbContext;
            GlobalOptions = globalOptions.CurrentValue;
        }

        public async Task<IEnumerable<Models.UI.Package>> GetFromQuery(bool doIncludePreview = false, string categoryId = null)
        {
            var q = DbContext.Packages.AsNoTracking();

            if (categoryId != null)
            {
                q = q.Where(p => p.PackageCategories.Any(c => c.CategoryId == categoryId));
            }

            return await q
                .Where(p => 
                    p.Visible && 
                    (doIncludePreview || !p.Preview) &&
                    p.PackagePlatforms.Any(pp => pp.PlatformId == GlobalOptions.PlatformId)
                )
                .Select(p => p.ToUiFormat(GlobalOptions.ProvisioningPageBaseUrl, true))
                .OrderBy(p => p.DisplayName)
                .ToListAsync();
        }

        public async Task<Models.UI.Package> GetById(Guid id, bool doIncludePreview = false)
        {
            return await DbContext.Packages.AsNoTracking()
                .Where(p =>
                    p.Visible &&
                    (doIncludePreview || !p.Preview) &&
                    p.PackagePlatforms.Any(pp => pp.PlatformId == GlobalOptions.PlatformId)
                )
                .Where(p => p.Id == id)
                .Select(p => p.ToUiFormat(GlobalOptions.ProvisioningPageBaseUrl, true))
                .FirstOrDefaultAsync();
        }
    }
}
