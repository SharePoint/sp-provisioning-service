using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using Newtonsoft.Json;

namespace SharePointPnP.ProvisioningApp.WebApi.Controllers
{
    // [Authorize]
    public class ProvisioningServiceController : ApiController
    {
        ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        /// <summary>
        /// Returns the parameters for a package
        /// </summary>
        /// <param name="id">The ID of the target package</param>
        /// <returns>A collection of parameters for the package</returns>
        [Route("api/ProvisioningService/GetPackageParameters")]
        public async Task<List<PackageParameter>> GetPackageParameters(Guid id)
        {
            // TODO: Manage authorization rules

            // Prepare the output object
            var result = new List<PackageParameter>();

            // Retrieve the package from the database
            var package = await dbContext.Packages.FirstOrDefaultAsync(p => p.Id == id);

            // If we found the package
            if (package != null)
            {
                // Deserialize the properties
                var properties = JsonConvert.DeserializeObject<Dictionary<String, String>>(package.PackageProperties);

                // Configure the metadata properties template
                var metadataTemplate = new
                {
                    properties = new[] {
                                    new {
                                        name = "",
                                        caption = "",
                                        description = "",
                                        editor = "",
                                        editorSettings = "",
                                    }
                                }
                };

                // And deserialize and process the metadata of properties
                var metadataProperties = JsonConvert.DeserializeAnonymousType(package.PropertiesMetadata, metadataTemplate);
                var metadata = metadataProperties.properties.ToDictionary(i => i.name);

                // Process every property
                foreach (var p in properties)
                {
                    // Get the mapping metadata, if any
                    var m = metadata.ContainsKey(p.Key) ? metadata[p.Key] : null;

                    // Build a single item for the output
                    result.Add(new PackageParameter
                    {
                        Name = p.Key,
                        DefaultValue = p.Value,
                        Caption =  m?.caption,
                        Description = m?.description,
                        Editor = m?.editor,
                        EditorSettings = m?.editorSettings
                    });
                }
            }

            // Return the generated output
            return (result);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}