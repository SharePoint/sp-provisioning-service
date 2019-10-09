//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
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
using System.Text.RegularExpressions;

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
        private const string PAGE_TEMPLATES_PATH = "system/pageTemplates";
        private const string SITE_PATH = "site";
        private const string TENANT_PATH = "tenant";
        private const string CATEGORIES_NAME = "categories.json";
        private const string PLATFORMS_NAME = "platforms.json";
        private const string TENANTS_NAME = "tenants.json";
        private const string SETTINGS_NAME = "settings.json";
        private const string INSTRUCTIONS_NAME = "instructions.md";
        private const string PROVISIONING_NAME = "provisioning.md";
        private const string README_NAME = "readme.md";

        public SyncEngine(ITemplatesProvider sourceProvider, ITemplatesProvider cloneProvider)
        {
            _sourceProvider = sourceProvider ?? throw new ArgumentNullException(nameof(sourceProvider));
            _cloneProvider = cloneProvider ?? throw new ArgumentNullException(nameof(cloneProvider));
        }

        public Action<string> Log { get; set; }

        public string ExclusionRules { get; set; }

        public async Task RunAsync(bool clone)
        {
            if (clone)
            {
                bool enableDiagnostics;
                bool.TryParse(ConfigurationManager.AppSettings["EnableDiagnostics"], out enableDiagnostics);
                WriteLog("Cloning source...");
                await _cloneProvider.CloneAsync(_sourceProvider, enableDiagnostics ? (Action<String>)WriteLog : (s) => { }, this.ExclusionRules);
            }

            // Where all source resources are based
            _baseSourceUri = FixRelativeTo((await _sourceProvider.GetAsync("", WriteLog)).OfType<ITemplateFile>().First().DownloadUri);
            _baseCloneUri = FixRelativeTo((await _cloneProvider.GetAsync("", WriteLog)).OfType<ITemplateFile>().First().DownloadUri);

            WriteLog("Synchonizing first release tenants...");
            await SyncFirstReleaseTenants();

            WriteLog("Synchonizing pages...");
            await SyncContentPages();

            WriteLog("Synchonizing page templates...");
            await SyncPageTemplates();

            WriteLog("Synchonizing categories...");
            await SyncCategories();

            WriteLog("Synchonizing platforms...");
            await SyncPlatforms();

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

        private async Task SyncPageTemplates()
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                // Find the category file
                var pageTemplatesFolders = await _cloneProvider.GetAsync(PAGE_TEMPLATES_PATH, WriteLog);
                if (pageTemplatesFolders == null || pageTemplatesFolders.Count() == 0) throw new InvalidOperationException($"Cannot find Page Template folders in folder {PAGE_TEMPLATES_PATH}");

                var existingDbPageTemplates = context.PageTemplates.ToDictionary(cp => cp.Id, StringComparer.OrdinalIgnoreCase);
                foreach (ITemplateFolder pageTemplateFolder in pageTemplatesFolders)
                {
                    // Prepare content variables
                    String htmlContent = null;
                    String cssContent = null;

                    // Get the page template folder content files
                    var pageTemplateFiles = await _cloneProvider.GetAsync(pageTemplateFolder.Path, WriteLog);
                    if (pageTemplateFiles == null || pageTemplateFiles.Count() == 0) throw new InvalidOperationException($"Cannot find template files in Page Template folder {pageTemplateFolder.Path}");

                    foreach (ITemplateFile pageTemplateFile in pageTemplateFiles)
                    {
                        // If the content file is an HTML file
                        if (pageTemplateFile.Path.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Set the HTML content
                            htmlContent = await GetFileContentAsync(pageTemplateFolder.Path, pageTemplateFile.Path.Substring(pageTemplateFile.Path.LastIndexOf('/') + 1));
                        }
                        else if (pageTemplateFile.Path.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Set the CSS content
                            cssContent = await GetFileContentAsync(pageTemplateFolder.Path, pageTemplateFile.Path.Substring(pageTemplateFile.Path.LastIndexOf('/') + 1));
                        }

                        if (!String.IsNullOrEmpty(htmlContent) && !String.IsNullOrEmpty(cssContent))
                        {
                            // If we have both HTML and CSS content, just break the foreach loop
                            break;
                        }
                    }

                    // Update Page Template, if already exists
                    if (existingDbPageTemplates.TryGetValue(pageTemplateFolder.Path, out PageTemplate dbPageTemplate))
                    {
                        dbPageTemplate.Html = htmlContent;
                        dbPageTemplate.Css = cssContent;
                        context.Entry(dbPageTemplate).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new Content Page
                        dbPageTemplate = new PageTemplate
                        {
                            Id = pageTemplateFolder.Path,
                            Html = htmlContent,
                            Css = cssContent,
                        };
                        context.Entry(dbPageTemplate).State = EntityState.Added;
                    }

                    existingDbPageTemplates.Remove(pageTemplateFolder.Path);
                }

                // Remove exceed categories
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                foreach (var dbPageTemplate in existingDbPageTemplates)
                {
                    context.Entry(dbPageTemplate.Value).State = EntityState.Deleted;
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

                int categoryOrder = 0;
                var existingDbCategories = context.Categories.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
                foreach (var category in categories.categories)
                {
                    // Update category, if already exists
                    if (existingDbCategories.TryGetValue(category.id, out Category dbCategory))
                    {
                        dbCategory.DisplayName = category.displayName;
                        dbCategory.Order = categoryOrder;
                        context.Entry(dbCategory).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new category
                        dbCategory = new Category
                        {
                            Id = category.id,
                            DisplayName = category.displayName,
                            Order = categoryOrder,
                        };
                        context.Entry(dbCategory).State = EntityState.Added;
                    }

                    categoryOrder++;
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

        private async Task SyncPlatforms()
        {
            using (ProvisioningAppDBContext context = GetContext())
            {
                // Find the platforms file
                ITemplateFile file = (await _cloneProvider.GetAsync(SYSTEM_PATH, WriteLog)).FindFile(PLATFORMS_NAME);
                if (file == null) throw new InvalidOperationException($"Cannot find file {PLATFORMS_NAME}");

                // Deserialize the json
                var platforms = await file.DownloadAsJsonAsync(new
                {
                    platforms = new[]
                        {
                            new {id = "", displayName = ""}
                        }
                });

                var existingDbPlatforms = context.Platforms.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
                foreach (var platform in platforms.platforms)
                {
                    // Update platform, if already exists
                    if (existingDbPlatforms.TryGetValue(platform.id, out Platform dbPlatform))
                    {
                        dbPlatform.DisplayName = platform.displayName;
                        context.Entry(dbPlatform).State = EntityState.Modified;
                    }
                    else
                    {
                        // Add new platform
                        dbPlatform = new Platform
                        {
                            Id = platform.id,
                            DisplayName = platform.displayName,
                        };
                        context.Entry(dbPlatform).State = EntityState.Added;
                    }

                    existingDbPlatforms.Remove(platform.id);
                }

                // Remove exceed platforms
                var objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                foreach (var dbPlatform in existingDbPlatforms)
                {
                    await context.Entry(dbPlatform.Value).Collection(p => p.Packages).LoadAsync();

                    foreach (var dbPackage in dbPlatform.Value.Packages.ToArray())
                    {
                        objectStateManager.ChangeRelationshipState(dbPlatform.Value, dbPackage, p => p.Packages, EntityState.Deleted);
                    }

                    context.Entry(dbPlatform.Value).State = EntityState.Deleted;
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
                    .Include(c => c.TargetPlatforms)
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

                        // New properties for Wave 4
                        dbPackage.Instructions = package.Instructions;
                        dbPackage.ProvisionRecap = package.ProvisionRecap;

                        // New properties for Wave 5
                        dbPackage.SortOrder = package.SortOrder;
                        dbPackage.SortOrderPromoted = package.SortOrderPromoted;
                        dbPackage.RepositoryRelativeUrl = package.RepositoryRelativeUrl;
                        dbPackage.Abstract = package.Abstract;

                        // New properties for Wave 6
                        dbPackage.ForceNewSite = package.ForceNewSite;
                        dbPackage.MatchingSiteBaseTemplateId = package.MatchingSiteBaseTemplateId;

                        // New properties for Wave 7
                        dbPackage.Visible = package.Visible;

                        // New properties for Wave 8
                        dbPackage.PackageProperties = package.PackageProperties;

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

                        // Add new platforms
                        foreach (var c in package.TargetPlatforms.Except(dbPackage.TargetPlatforms).ToArray())
                        {
                            objectStateManager.ChangeRelationshipState(dbPackage, c, d => d.TargetPlatforms, EntityState.Added);
                        }
                        // Remove old platforms
                        foreach (var c in dbPackage.TargetPlatforms.Except(package.TargetPlatforms).ToArray())
                        {
                            objectStateManager.ChangeRelationshipState(dbPackage, c, d => d.TargetPlatforms, EntityState.Deleted);
                        }

                        existingDbPackages.Remove(dbPackage.PackageUrl);
                    }
                    else
                    {
                        // Add new package
                        context.Entry(package).State = EntityState.Added;
                    }
                }

                // Remove leftover packages
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

            #region Prepare the settings file and its outline

            var settings = await settingsFile.DownloadAsJsonAsync(new TemplateSettings());

            #endregion

            // Read the package file
            ITemplateFile packageFile = items.FindFile(settings.packageFile);
            if (packageFile == null)
            {
                WriteLog($"Cannot find file {settings.packageFile}");
                return null;
            }

            #region Fix the Preview Image URLs

            // Fix the URLs in the metadata settings
            if (settings?.metadata?.displayInfo?.previewImages != null)
            {
                int previewImagesCount = settings.metadata.displayInfo.previewImages.Length;
                for (var n = 0; n < previewImagesCount; n++)
                {
                    var previewImageUrl = settings.metadata.displayInfo.previewImages[n].url;
                    settings.metadata.displayInfo.previewImages[n].url = 
                        ChangeUri(packageFile.DownloadUri, previewImageUrl);
                }
            }

            if (settings?.metadata?.displayInfo?.detailItemCategories != null)
            {
                int detailItemCategoriesCount = settings.metadata.displayInfo.detailItemCategories.Length;
                for (var n = 0; n < detailItemCategoriesCount; n++)
                {
                    if (settings.metadata.displayInfo.detailItemCategories[n].items != null)
                    {
                        var detailItemCategoryItemsCount = settings.metadata.displayInfo.detailItemCategories[n].items.Length;
                        for (var m = 0; m < detailItemCategoryItemsCount; m++)
                        {
                            var detailItemCategoryItemPreviewImageUrl = settings.metadata.displayInfo.detailItemCategories[n].items[m].previewImage;
                            if (!String.IsNullOrEmpty(detailItemCategoryItemPreviewImageUrl))
                            {
                                settings.metadata.displayInfo.detailItemCategories[n].items[m].previewImage = 
                                    ChangeUri(packageFile.DownloadUri, detailItemCategoryItemPreviewImageUrl);
                            }
                        }
                    }
                }
            }

            #endregion

            var package = new DomainModel.Package
            {
                Id = Guid.NewGuid(),
                PackageUrl = packageFile.DownloadUri.ToString(),
                // New properties for Wave2
                Promoted = settings.promoted,
                Preview = settings.preview,
                TimesApplied = 0,
                PropertiesMetadata = JsonConvert.SerializeObject(settings.metadata),
                // New properties for Wave 5
                Abstract = settings.@abstract,
                SortOrder = settings.sortOrder,
                SortOrderPromoted = settings.sortOrderPromoted,
                RepositoryRelativeUrl = folder.Path,
                MatchingSiteBaseTemplateId = settings.matchingSiteBaseTemplateId,
                ForceNewSite = settings.forceNewSite,
                Visible = settings.visible,
                PageTemplateId = settings.metadata?.displayInfo?.pageTemplateId,
            };

            // Read the instructions.md and the provisioning.md files
            String instructionsContent = await GetHtmlContentAsync(folder.Path, INSTRUCTIONS_NAME);
            String provisioningContent = await GetHtmlContentAsync(folder.Path, PROVISIONING_NAME);

            package.Instructions = instructionsContent;
            package.ProvisionRecap = provisioningContent;

            // Find the categories to apply
            if (settings.categories != null && settings.categories.Length > 0)
            {
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
            }

            // Find the platforms to apply
            if (settings.platforms != null && settings.platforms.Length > 0)
            {
                var dbPlatforms = settings.platforms.Select(p =>
                {
                    Platform dbPlatform = context.Platforms.Find(p);
                    if (dbPlatform == null)
                    {
                        WriteLog($"Cannot find platform with id {p}");
                    }

                    return dbPlatform;
                }).ToArray();
                package.TargetPlatforms.AddRange(dbPlatforms);
            }

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

                    // Get a provider based on the in-memory .PNP Open XML file
                    OpenXMLConnector openXmlConnector = new OpenXMLConnector(mem);
                    XMLTemplateProvider provider = new XMLOpenXMLTemplateProvider(
                        openXmlConnector);

                    // Get the .xml provisioning template file name
                    var xmlTemplateFileName = openXmlConnector.Info?.Properties?.TemplateFileName ??
                        packageFile.Path.Substring(packageFile.Path.LastIndexOf('/') + 1)
                        .ToLower().Replace(".pnp", ".xml");

                    // Get the full hierarchy
                    hierarchy = provider.GetHierarchy(xmlTemplateFileName);
                }

                if (hierarchy != null)
                {
                    package.DisplayName = hierarchy.DisplayName;
                    package.ImagePreviewUrl = ChangeUri(packageFile.DownloadUri, hierarchy?.ImagePreviewUrl ?? String.Empty);
                    package.Description = await GetDescriptionAsync(packageFile.GetDirectoryPath()) ?? hierarchy?.Description ?? "";
                    package.Version = hierarchy?.Version.ToString();
                    package.PackageProperties = JsonConvert.SerializeObject(hierarchy.Parameters);
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

        private async Task<string> GetFileContentAsync(string path, string fileName)
        {
            ITemplateFile contentFile = (await _sourceProvider.GetAsync(path, WriteLog)).FindFile(fileName);
            if (contentFile == null) return null;

            ITemplateFile cloneContentFile = (await _cloneProvider.GetAsync(path, WriteLog)).FindFile(fileName);

            // Get the file content
            using (var httpClient = new HttpClient())
            {
                // Get the content
                var contentStream = await contentFile.DownloadAsync();
                using (var sr = new StreamReader(contentStream))
                {
                    var content = await sr.ReadToEndAsync();

                    // Change relative uris
                    return ChangeUris(cloneContentFile.DownloadUri, content);
                }
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
            htmlDoc.OptionOutputOriginalCase = true;
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
