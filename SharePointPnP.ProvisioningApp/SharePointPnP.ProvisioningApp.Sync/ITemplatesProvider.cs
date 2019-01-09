using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    public interface ITemplatesProvider
    {
        Task<IEnumerable<ITemplateItem>> GetAsync(string path);

        Task CloneAsync(ITemplatesProvider sourceProvider);

    }
}
