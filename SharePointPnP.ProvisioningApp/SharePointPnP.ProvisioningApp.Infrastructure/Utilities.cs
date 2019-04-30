//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    public class Utilities
    {
        /// <summary>
        /// Helper function to load an X509 certificate
        /// </summary>
        /// <param name="certificateThumbprint">Thumbprint of the certificate to be loaded</param>
        /// <param name="certificateStoreName">Store name of the certificate to be loaded</param>
        /// <param name="certificateStoreLocation">Store location of the certificate to be loaded</param>
        /// <returns>X509 Certificate</returns>
        /// <remarks>All input arguments are required, missing of an input argument result in an ArgumentNullException</remarks>
        public static X509Certificate2 FindCertificateByThumbprint(string certificateThumbprint, string certificateStoreName, string certificateStoreLocation)
        {
            if (String.IsNullOrEmpty(certificateThumbprint))
                throw new System.ArgumentNullException(nameof(certificateThumbprint));

            if (String.IsNullOrEmpty(certificateStoreName))
                throw new System.ArgumentNullException(nameof(certificateStoreName));

            if (String.IsNullOrEmpty(certificateStoreLocation))
                throw new System.ArgumentNullException(nameof(certificateStoreLocation));

            X509Certificate2 appOnlyCertificate = null;

            Enum.TryParse(certificateStoreName, out StoreName storeName);
            Enum.TryParse(certificateStoreLocation, out StoreLocation storeLocation);

            X509Store certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    certificateThumbprint,
                    false);

            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                appOnlyCertificate = certCollection[0];
            }
            certStore.Close();

            if (appOnlyCertificate == null)
            {
                throw new System.Exception(
                        string.Format("Could not find the certificate with thumbprint {0} in certificate store {1} in location {2}.", certificateThumbprint, certificateStoreName, certificateStoreLocation));
            }

            return appOnlyCertificate;
        }

        private static String GlobalTenantAdminRole = "Company Administrator";
        private static String GlobalSPOAdminRole = "SharePoint Service Administrator";

        public static Boolean UserIsTenantGlobalAdmin(String graphAccessToken)
        {
            return (UserIsAdmin(Utilities.GlobalTenantAdminRole, graphAccessToken));
        }

        public static Boolean UserIsSPOAdmin(String graphAccessToken)
        {
            return (UserIsAdmin(Utilities.GlobalSPOAdminRole, graphAccessToken));
        }

        private static Boolean UserIsAdmin(String targetRole, String graphAccessToken)
        {
            try
            {
                // Retrieve (using the Microsoft Graph) the current user's roles
                String jsonResponse = HttpHelper.MakeGetRequestForString(
                    "https://graph.microsoft.com/v1.0/me/memberOf?$select=id,displayName",
                    graphAccessToken);

                if (jsonResponse != null)
                {
                    var result = JsonConvert.DeserializeObject<UserRoles>(jsonResponse);
                    // Check if the requested role (of type DirectoryRole) is included in the list
                    return (result.Roles.Any(r => r.DisplayName == targetRole &&
                        r.DataType.Equals("#microsoft.graph.directoryRole", StringComparison.InvariantCultureIgnoreCase)));
                }
            }
            catch (Exception)
            {
                // Ignore any exception and return false (user is not member of ...)
            }

            return (false);
        }
    }
}
