using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.Providers.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    public class XMLAzureStorageTemplateProvider : XMLTemplateProvider
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public XMLAzureStorageTemplateProvider() : base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="container"></param>
        public XMLAzureStorageTemplateProvider(string connectionString, string container) :
            base(new AzureStorageConnector(connectionString, container))
        {
        }
    }
}
