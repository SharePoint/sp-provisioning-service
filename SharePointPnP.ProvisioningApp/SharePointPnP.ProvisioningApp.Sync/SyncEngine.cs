using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.DomainModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using System.Xml.Linq;
using OfficeDevPnP.Core.Framework.Provisioning.Providers;
using System.Configuration;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using HtmlAgilityPack;
using System.Net.Http;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    public class SyncEngine
    {
        private readonly ITemplatesProvider _sourceProvider;
        private readonly ITemplatesProvider _cloneProvider;
        private Uri _baseSourceUri;
        private Uri _baseCloneUri;
        private const string SYSTEM_PATH = "system";
        private const string CONTENT_PATH = "system/pages";
        private const string SITE_PATH = "site";
        private const string TENANT_PATH = "tenant";
        private const string CATEGORIES_NAME = "categories.json";
        private const string TENANTS_NAME = "tenants.json";
        private const string SETTINGS_NAME = "settings.json";
        private const string README_NAME = "readme.md";

        public SyncEngine(ITemplatesProvider sourceProvider, ITemplatesProvider cloneProvider)
        {
            _sourceProvider = sourceProvider ?? throw new ArgumentNullException(nameof(sourceProvider));
            _cloneProvider = cloneProvider ?? throw new ArgumentNullException(nameof(cloneProvider));
        }

        public Action<string> Log { get; set; }

        public async Task RunAsync(bool clone)
        {
            if (clone)
            {
                bool enableDiagnostics;
                bool.TryParse(ConfigurationManager.AppSettings["EnableDiagnostics"], out enableDiagnostics);
                WriteLog("Cloning source...");
                await _cloneProvider.CloneAsync(_sourceProvider, enableDiagnostics ? (Action<String>)WriteLog : (s) => { });
            }

            // Where all source resources are based
            _baseSourceUri = FixRelativeTo((await _sourceProvider.GetAsync("", WriteLog)).OfType<ITemplateFile>().First().DownloadUri);
            _baseCloneUri = FixRelativeTo((await _cloneProvider.GetAsync("", WriteLog)).OfType<ITemplateFile>().First().DownloadUri);

            WriteLog("Synchonizing first release tenants...");
            await SyncFirstReleaseTenants();

            WriteLog("Synchonizing pages...");
            await SyncContentPages();

            WriteLog("Synchonizing categories...");
            await SyncCategories();

            WriteLog("Synchonizing templates...");
            await SyncTemplatesAsync(SITE_PATH, PackageType.SiteCollection);
            await SyncTemplatesAsync(TENANT_PATH, PackageType.Tenant);

            WriteLog("Done");
        }

        private async Task SyncFirstReleaseTenants()
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                // Find the category file
                ITemplateFile file = (await _cloneProvider.GetAsync(SYSTEM_PATH, WriteLog)).FindFile(TENANTS_NAME);
                if (file == null) throw new InvalidOperationException($"Cannot find file {TENANTS_NAME}");

                // Deserialize the json
                var tenantsList = await file.DownloadAsJsonAsync(new
                {
                    tenants = new[]
                        {
                            new {id = "", tenantName = "", referenceOwner = ""}
                        }
                });

                var existingDbTenants = context.Tenants.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
                foreach (var tenant in tenantsList.tenants)
                {
                    // Update tenant, if already exists
                    if (existingDbTenants.TryGetValue(tenant.id, out Tenant dbTenant))
                    {
                        dbTenant.TenantName = tenant.tenantName;
                        dbTenant.ReferenceOwner = tenant.referenceOwner;
                        context.Entry(dbTenant).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new tenant
                        dbTenant = new Tenant
                        {
                            Id = tenant.id,
                            TenantName = tenant.tenantName,
                            ReferenceOwner = tenant.referenceOwner
                        };
                        context.Entry(dbTenant).State = EntityState.Added;
                    }

                    existingDbTenants.Remove(tenant.id);
                }

                // Remove exceed categories
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                foreach (var dbTenant in existingDbTenants)
                {
                    context.Entry(dbTenant.Value).State = EntityState.Deleted;
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task SyncContentPages()
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                // Find the category file
                var files = await _cloneProvider.GetAsync(CONTENT_PATH, WriteLog);
                if (files == null || files.Count() == 0) throw new InvalidOperationException($"Cannot find files in folder {CONTENT_PATH}");

                var existingDbContentPages = context.ContentPages.ToDictionary(cp => cp.Id, StringComparer.OrdinalIgnoreCase);
                foreach (ITemplateFile file in files)
                {
                    // Get the file content
                    String fileContent = await GetHtmlContentAsync(CONTENT_PATH, file.Path.Substring(file.Path.LastIndexOf('/') + 1));
    
                    // Update Content Page, if already exists
                    if (existingDbContentPages.TryGetValue(file.Path, out ContentPage dbContentPage))
                    {
                        dbContentPage.Content = fileContent;
                        context.Entry(dbContentPage).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new Content Page
                        dbContentPage = new ContentPage
                        {
                            Id = file.Path,
                            Content = fileContent,
                        };
                        context.Entry(dbContentPage).State = EntityState.Added;
                    }

                    existingDbContentPages.Remove(file.Path);
                }

                // Remove exceed categories
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                foreach (var dbContentPage in existingDbContentPages)
                {
                    context.Entry(dbContentPage.Value).State = EntityState.Deleted;
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task SyncCategories()
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                // Find the category file
                ITemplateFile file = (await _cloneProvider.GetAsync(SYSTEM_PATH, WriteLog)).FindFile(CATEGORIES_NAME);
                if (file == null) throw new InvalidOperationException($"Cannot find file {CATEGORIES_NAME}");

                // Deserialize the json
                var categories = await file.DownloadAsJsonAsync(new
                {
                    categories = new[]
                        {
                            new {id = "", displayName = ""}
                        }
                });

                var existingDbCategories = context.Categories.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
                foreach (var category in categories.categories)
                {
                    // Update category, if already exists
                    if (existingDbCategories.TryGetValue(category.id, out Category dbCategory))
                    {
                        dbCategory.DisplayName = category.displayName;
                        context.Entry(dbCategory).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new category
                        dbCategory = new Category
                        {
                            Id = category.id,
                            DisplayName = category.displayName
                        };
                        context.Entry(dbCategory).State = EntityState.Added;
                    }

                    existingDbCategories.Remove(category.id);
                }

                // Remove exceed categories
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                foreach (var dbCategory in existingDbCategories)
                {
                    await context.Entry(dbCategory.Value).Collection(d => d.Packages).LoadAsync();

                    foreach (var dbPackage in dbCategory.Value.Packages.ToArray())
                    {
                        objectStateManager.ChangeRelationshipState(dbCategory.Value, dbPackage, d => d.Packages, EntityState.Deleted);
                    }

                    context.Entry(dbCategory.Value).State = EntityState.Deleted;
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task SyncTemplatesAsync(string path, PackageType type)
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;

                // Find packages inside folder
                IEnumerable<ITemplateItem> items = await _cloneProvider.GetAsync(path, WriteLog);
                var packages = await FindPackagesAsync(items, context);

                var existingDbPackages = context.Packages
                    .Include(c => c.Categories)
                    .Where(p => p.PackageType == type)
                    .ToDictionary(c => c.PackageUrl, StringComparer.OrdinalIgnoreCase);
                foreach (DomainModel.Package package in packages)
                {
                    package.PackageType = type;

                    // Update package, if already exists
                    if (existingDbPackages.TryGetValue(package.PackageUrl, out DomainModel.Package dbPackage))
                    {
                        // Copy info into existing item
                        dbPackage.DisplayName = package.DisplayName;
                        dbPackage.Author = package.Author;
                        dbPackage.AuthorLink = package.AuthorLink;
                        dbPackage.Description = package.Description;
                        dbPackage.ImagePreviewUrl = package.ImagePreviewUrl;
                        dbPackage.PackageUrl = package.PackageUrl;
                        dbPackage.Version = package.Version;

                        // New properties for Wave2
                        dbPackage.Promoted = package.Promoted;
                        dbPackage.Preview = package.Preview;
                        dbPackage.PropertiesMetadata = package.PropertiesMetadata;
                        
                        // Keep times applied from the DB
                        // dbPackage.TimesApplied = package.TimesApplied;

                        context.Entry(dbPackage).State = EntityState.Modified;

                        // Add new categories
                        foreach (var c in package.Categories.Except(dbPackage.Categories).ToArray())
                        {
                            objectStateManager.ChangeRelationshipState(dbPackage, c, d => d.Categories, EntityState.Added);
                        }
                        // Remove old categories
                        foreach (var c in dbPackage.Categories.Except(package.Categories).ToArray())
                        {
                            objectStateManager.ChangeRelationshipState(dbPackage, c, d => d.Categories, EntityState.Deleted);
                        }

                        existingDbPackages.Remove(dbPackage.PackageUrl);
                    }
                    else
                    {
                        // Add new package
                        context.Entry(package).State = EntityState.Added;
                    }
                }

                // Remove exceed packages
                foreach (var dbPackage in existingDbPackages)
                {
                    await context.Entry(dbPackage.Value).Collection(d => d.Categories).LoadAsync();

                    foreach (var dbCategory in dbPackage.Value.Categories.ToArray())
                    {
                        objectStateManager.ChangeRelationshipState(dbPackage.Value, dbCategory, d => d.Categories, EntityState.Deleted);
                    }

                    context.Entry(dbPackage.Value).State = EntityState.Deleted;
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task<IReadOnlyList<DomainModel.Package>> FindPackagesAsync(IEnumerable<ITemplateItem> items, ProvisioningAppDBContext context)
        {
            var packages = new List<DomainModel.Package>();
            foreach (ITemplateFolder folder in items.OfType<ITemplateFolder>())
            {
                WriteLog($"Processing folder {folder.Path}...");

                try
                {
                    DomainModel.Package package = await GetPackageAsync(folder, context);
                    if (package == null) continue;

                    if (!String.IsNullOrEmpty(package.DisplayName))
                    {
                        packages.Add(package);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Error processing folder {folder.Path}: {ex.Message} - {ex.StackTrace}");
                }
            }

            return packages;
        }

        private async Task<DomainModel.Package> GetPackageAsync(ITemplateFolder folder, ProvisioningAppDBContext context)
        {
            var items = await _cloneProvider.GetAsync(folder.Path, WriteLog);

            // Read settings file
            ITemplateFile settingsFile = items.OfType<ITemplateFile>().FindFile(SETTINGS_NAME);
            if (settingsFile == null)
            {
                WriteLog($"Cannot find file {SETTINGS_NAME}");
                return null;
            }

            var settings = await settingsFile.DownloadAsJsonAsync(new
            {
                categories = new string[0],
                packageFile = "",
                promoted = false,
                preview = false,
                metadata = new {
                    properties = new[] { 
                        new {
                            name = "",
                            caption = "",
                            description = "",
                        }
                    }
                }
            });

            // Read the package file
            ITemplateFile packageFile = items.FindFile(settings.packageFile);
            if (packageFile == null)
            {
                WriteLog($"Cannot find file {settings.packageFile}");
                return null;
            }

            var package = new DomainModel.Package
            {
                Id = Guid.NewGuid(),
                PackageUrl = packageFile.DownloadUri.ToString(),
                // New properties for Wave2
                Promoted = settings.promoted,
                Preview = settings.preview,
                TimesApplied = 0,
                PropertiesMetadata = JsonConvert.SerializeObject(settings.metadata)
            };

            // Find the categories to apply
            var dbCategories = settings.categories.Select(c =>
            {
                Category dbCategory = context.Categories.Find(c);
                if (dbCategory == null)
                {
                    WriteLog($"Cannot find category with id {c}");
                }

                return dbCategory;
            }).ToArray();
            package.Categories.AddRange(dbCategories);

            // Find then author and fill his informations
            await FillAuthorAsync(package, packageFile.Path);

            // Open the package and set info
            await FillPackageAsync(package, packageFile);

            return package;
        }

        private async Task FillPackageAsync(DomainModel.Package package, ITemplateFile packageFile)
        {
            using (Stream stream = await packageFile.DownloadAsync())
            {
                // Crate a copy of the source stream
                MemoryStream mem = new MemoryStream();
                await stream.CopyToAsync(mem);
                mem.Position = 0;

                // Prepare the output hierarchy
                ProvisioningHierarchy hierarchy = null;

                if (packageFile.Path.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    // That's an XML Provisioning Template file

                    XDocument xml = XDocument.Load(mem);
                    mem.Position = 0;

                    // Deserialize the stream into a provisioning hierarchy reading any 
                    // dependecy with the Azure Blob Storage connector
                    var formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(xml.Root.Name.NamespaceName);
                    var templateLocalFolder = $"{ConfigurationManager.AppSettings["BlobTemplatesProvider:ContainerName"]}/{packageFile.Path.Substring(0, packageFile.Path.LastIndexOf('/'))}";

                    var provider = new XMLAzureStorageTemplateProvider(
                        ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"],
                        templateLocalFolder);
                    formatter.Initialize(provider);

                    // Get the full hierarchy
                    hierarchy = ((IProvisioningHierarchyFormatter)formatter).ToProvisioningHierarchy(mem);
                }
                else if (packageFile.Path.EndsWith(".pnp", StringComparison.InvariantCultureIgnoreCase))
                {
                    // That's a PnP Package file

                    // Get the .xml provisioning template file name
                    var xmlTemplateFileName = packageFile.Path.Substring(packageFile.Path.LastIndexOf('/') + 1)
                        .ToLower().Replace(".pnp", ".xml");

                    // Get a provider based on the in-memory .PNP Open XML file
                    XMLTemplateProvider provider = new XMLOpenXMLTemplateProvider(
                        new OpenXMLConnector(mem));

                    // Get the full hierarchy
                    hierarchy = provider.GetHierarchy(xmlTemplateFileName);
                }

                if (hierarchy != null)
                {
                    package.DisplayName = hierarchy.DisplayName;
                    package.ImagePreviewUrl = ChangeUri(packageFile.DownloadUri, hierarchy?.ImagePreviewUrl ?? String.Empty);
                    package.Description = await GetDescriptionAsync(packageFile.GetDirectoryPath()) ?? hierarchy?.Description ?? "";
                    package.Version = hierarchy?.Version.ToString();
                }
            }
        }

        private string ChangeUri(Uri relativeTo, string uri)
        {
            relativeTo = FixRelativeTo(relativeTo);

            if (Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri u))
            {
                if (u.IsAbsoluteUri && _baseSourceUri.IsBaseOf(u))
                {
                    return new Uri(_baseCloneUri, _baseSourceUri.MakeRelativeUri(u)).ToString();
                }
                else
                {
                    return new Uri(relativeTo, uri).ToString();
                }
            }
            return uri;
        }

        private async Task<string> GetDescriptionAsync(string path)
        {
            IMarkdownFile readmeFile = (await _sourceProvider.GetAsync(path, WriteLog)).FindFile(README_NAME) as IMarkdownFile;
            if (readmeFile == null) return null;

            ITemplateFile cloneReadmeFile = (await _cloneProvider.GetAsync(path, WriteLog)).FindFile(README_NAME);

            // Get the markdown
            using (var httpClient = new HttpClient())
            {
                // Get html
                string html = await readmeFile.GetHtmlAsync();
                // Change relative uris
                return ChangeUris(cloneReadmeFile.DownloadUri, html);
            }
        }

        private async Task<string> GetHtmlContentAsync(string path, string fileName)
        {
            IMarkdownFile contentFile = (await _sourceProvider.GetAsync(path, WriteLog)).FindFile(fileName) as IMarkdownFile;
            if (contentFile == null) return null;

            ITemplateFile cloneContentFile = (await _cloneProvider.GetAsync(path, WriteLog)).FindFile(fileName);

            // Get the markdown
            using (var httpClient = new HttpClient())
            {
                // Get html
                string html = await contentFile.GetHtmlAsync();
                // Change relative uris
                return ChangeUris(cloneContentFile.DownloadUri, html);
            }
        }

        private Uri FixRelativeTo(Uri relativeTo)
        {
            if (!relativeTo.AbsolutePath.EndsWith("/"))
            {
                relativeTo = new Uri(relativeTo.ToString().Substring(0, relativeTo.ToString().LastIndexOf("/") + 1));
            }

            return relativeTo;
        }

        private string ChangeUris(Uri relativeTo, string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            relativeTo = FixRelativeTo(relativeTo);

            foreach (var img in (IEnumerable<HtmlNode>)htmlDoc.DocumentNode.SelectNodes("//img") ?? new HtmlNode[0])
            {
                var src = img.GetAttributeValue("src", String.Empty);
                Uri u;
                if (Uri.TryCreate(src, UriKind.Relative, out u))
                {
                    // Create a full URI
                    src = new Uri(relativeTo, u).ToString();
                    img.SetAttributeValue("src", src);
                }
            }

            foreach (var img in (IEnumerable<HtmlNode>)htmlDoc.DocumentNode.SelectNodes("//a") ?? new HtmlNode[0])
            {
                var href = img.GetAttributeValue("href", String.Empty);
                Uri u;
                if (!href.StartsWith("#") && Uri.TryCreate(href, UriKind.Relative, out u))
                {
                    // Create a full URI
                    href = new Uri(relativeTo, u).ToString();
                    img.SetAttributeValue("href", href);
                }
            }

            var result = new StringWriter();
            htmlDoc.Save(result);

            return result.ToString();
        }

        private async Task FillAuthorAsync(DomainModel.Package package, string path)
        {
            if (!(_sourceProvider is IAuthorProvider ap)) return;

            ITemplateAuthor author = await ap.GetAuthorAsync(path);
            if (author == null) return;
            package.Author = author.Name;
            package.AuthorLink = author.Link;
        }

        private ProvisioningAppDBContext GetContext()
        {
            var context = new ProvisioningAppDBContext();
            context.Configuration.ProxyCreationEnabled = false;
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.AutoDetectChangesEnabled = false;

            return context;
        }

        private void WriteLog(string message)
        {
            Log?.Invoke(message);
        }
    }
}
