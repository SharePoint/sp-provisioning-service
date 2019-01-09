using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// This type is the entry point for all the providers offered
    /// by the Infrastructure project of SharePointPnP.ProvisioningApp
    /// </summary>
    public static class ProvisioningAppManager
    {
        private static readonly Lazy<IAccessTokenProvider> _accessTokenProvider = new Lazy<IAccessTokenProvider>(() => {
            return (ProvisioningAppManager.CreateProviderInstance<IAccessTokenProvider>(ConfigurationManager.AppSettings["SPPA:AccessTokenProvider"]));
        });

        public static IAccessTokenProvider AccessTokenProvider
        {
            get { return (_accessTokenProvider.Value); }
        }

        private static TProvider CreateProviderInstance<TProvider>(String providerTypeName)
            where TProvider : class
        {
            Type providerType = Type.GetType(providerTypeName, true);
            var provider = Activator.CreateInstance(providerType) as TProvider;
            return (provider);
        }

        public static bool IsTestingEnvironment
        {
            // Default value: false
            get { return (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"] ?? "false")); }
        }
    }
}
