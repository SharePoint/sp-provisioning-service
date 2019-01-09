using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    public class BaseModel<TKey>
    {
        public TKey Id { get; set; }
    }
}
