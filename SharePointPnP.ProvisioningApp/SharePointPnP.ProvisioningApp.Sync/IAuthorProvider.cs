using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    public interface IAuthorProvider
    {
        Task<ITemplateAuthor> GetAuthorAsync(string path);
    }
}
