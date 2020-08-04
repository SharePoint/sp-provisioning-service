using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharePoint.Portal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Data
{
    public class PortalDbContext : DbContext
    {
        public PortalDbContext(DbContextOptions<PortalDbContext> contextOptions)
            : base(contextOptions)
        { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<PackageCategory> PackagesCategories { get; set; }

        public DbSet<PackagePlatform> PackagesPlatforms { get; set; }

        public DbSet<PageTemplate> PageTemplates { get; set; }

        public DbSet<Platform> Platforms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set the default schema to pnp
            modelBuilder.HasDefaultSchema("pnp");

            modelBuilder.Entity<PackageCategory>()
                .HasKey(pc => new { pc.PackageId, pc.CategoryId });

            modelBuilder.Entity<PackagePlatform>()
                .HasKey(pp => new { pp.PackageId, pp.PlatformId });

            modelBuilder.Entity<Package>()
                .Property(p => p.PropertiesMetadata)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<Models.UI.MetaData>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                );
        }


        public void SeedDevData()
        {
            Platforms.Add(new Platform
            {
                Id = "LOOKBOOK"
            });

            Categories.AddRange(new Category
            {
                Id = "ORGANIZATION",
                DisplayName = "Organization",
            },
            new Category
            {
                Id = "DEPARTMENT",
                DisplayName = "Department",
            },
            new Category
            {
                Id = "TEAM",
                DisplayName = "Team",
            },
            new Category
            {
                Id = "COMMUNITY",
                DisplayName = "Community",
            });


            PageTemplates.AddRange(
                new PageTemplate
                {
                    Id = "template-1",
                    Html = @"<div [slbDetailsPageTemplateContext]=""context"" class=""page-wrapper"">
        <section class=""container-fluid"">
            <div class=""row"">
                <div class=""col-lg-6 col-xl-5"">
                    <div class=""h7 text-uppercase mb-8px"">
                        <slb-package-title></slb-package-title>
                    </div>

                    <h4 class=""mb-32px"">
                        <slb-site-descriptor></slb-site-descriptor>
                    </h4>

                    <div>
                        <slb-package-description></slb-package-description>
                    </div>

                    <div class=""subsection template-info"">
                        <slb-expandable-panel *slbDisplayForCategory=""'Features'; let items=items"">
                            <slb-expandable-panel-header>
                                Site features
                            </slb-expandable-panel-header>

                            <div class=""mt-16px mb-24px"">
                                <ul class=""list-unstyled"">
                                    <li *ngFor=""let item of items"">
                                        <slb-detail-item [item]=""item""></slb-detail-item>
                                    </li>
                                </ul>
                            </div>
                        </slb-expandable-panel>

                        <slb-expandable-panel *slbDisplayForCategory=""'Webparts'; let items=items"">
                            <slb-expandable-panel-header>
                                Web parts used
                            </slb-expandable-panel-header>

                            <div class=""mt-16px mb-24px"">
                                <ul class=""list-unstyled"">
                                    <li *ngFor=""let item of items"">
                                        <slb-detail-item [item]=""item""></slb-detail-item>
                                    </li>
                                </ul>
                            </div>
                        </slb-expandable-panel>

                        <slb-expandable-panel *slbDisplayForCategory=""'Site Content'; let items=items"">
                            <slb-expandable-panel-header>
                                Content included
                            </slb-expandable-panel-header>

                            <div class=""mt-16px mb-24px"">
                                <p>Adding this design to your tenant will create the following content:</p>

                                <ul class=""mt-16px site-content-list"">
                                    <li *ngFor=""let item of items"">
                                        {{ item.name }}
                                    </li>
                                </ul>
                            </div>
                        </slb-expandable-panel>
                    </div>

                    <div class=""subsection slb-font-size-smaller add-to-tenant-subsection"">
                        <slb-sticky-header>
                            <div slbStickyHeaderContentClass=""container-fluid stickied-header-content"">
                                <div class=""d-flex w-100 align-items-center justify-content-between"">
                                    <h4 class=""m-0"">
                                        <slb-package-title></slb-package-title>
                                    </h4>
                                    <button slb-cta-button slbAddToTenantButton>Add to your tenant</button>
                                </div>
                            </div>
                        </slb-sticky-header>

                        <div class=""mt-16px"">
                            <slb-permission-message></slb-permission-message>
                        </div>
                    </div>

                    <div class=""subsection"" *slbDisplayForCategory=""'Resources'; let items=items"">
                        <h4>Resources</h4>

                        <p>
                            Whether you're starting from scratch or you've installed this example on your tenant, here
                            are some resources to help you create and personalize your site.
                        </p>

                        <div *ngFor=""let item of items"">
                            <a [href]=""item.url"">
                                {{ item.name }}
                                <p>
                                    {{ item.description }}
                                </p>
                            </a>
                        </div>
                    </div>

                    <div class=""subsection"">
                        <h4 class=""mb-3"">System Requirements</h4>
                        <slb-system-requirements-table class=""w-100""></slb-system-requirements-table>
                    </div>
                </div>

                <div class=""col-lg-6 col-xl-7 image-container"">
                    <picture class=""d-block animate-slide-left-fade-in"">
                        <img slbTemplatePreviewImage class=""w-100 box-shadow-elevation-1"" />
                    </picture>
                </div>
            </div>
        </section>
    </div>",
                    Css = @".page-wrapper {
    /* hide image sliding in from right offpage */
    overflow-x: hidden;
}

.image-container {
    margin-top: 64px;
}

@media (min-width: 992px) {
    .image-container {
        margin-top: 0;
    }
}

.add-to-tenant-subsection h4 {
    display: none;
}

.site-content-list {
    margin-bottom: 0;
    padding-inline-start: 30px;
}

li:not(:first-child) {
    margin-top: 20px;
}"
                },
                new PageTemplate
                {
                    Id = "template-2",
                    Html = @"<div [slbDetailsPageTemplateContext]=""context"" class=""page-wrapper"">
    <section class=""container-fluid"">
        <div class=""row flex-wrap-reverse"">
            <div class=""col-lg-6 col-xl-7"">
                <picture class=""d-block animate-slide-right-fade-in"">
                    <img slbTemplatePreviewImage class=""w-100 box-shadow-elevation-1"" />
                </picture>
            </div>

            <div class=""col-lg-6 col-xl-5"">
                <h1>
                    <slb-package-title></slb-package-title>
                </h1>

                <div class=""detail-text"">
                    <slb-package-description></slb-package-description>
                </div>

                <div class=""subsection slb-font-size-smaller add-to-tenant-subsection"">
                    <slb-sticky-header>
                        <div slbStickyHeaderContentClass=""container-fluid stickied-header-content"">
                            <div class=""d-flex w-100 align-items-center justify-content-between"">
                                <h1 class=""m-0"">
                                    <slb-package-title></slb-package-title>
                                </h1>
                                <button slb-cta-button slbAddToTenantButton>Add to your tenant</button>
                            </div>
                        </div>
                    </slb-sticky-header>
                </div>

                <div class=""subsection"" *ngFor=""let category of context.detailItemCategories"">
                    <h2>{{ category.name }}</h2>

                    <div class=""d-flex flex-wrap"">
                        <div class=""w-50 pr-3 mt-3"" *ngFor=""let item of category.items"">
                            <slb-detail-item [item]=""item""></slb-detail-item>
                        </div>
                    </div>
                </div>

                <div class=""subsection"">
                    <slb-permission-message></slb-permission-message>
                </div>

                <div class=""subsection"">
                    <h4 class=""mb-3"">System Requirements</h4>
                    <slb-system-requirements-table></slb-system-requirements-table>
                </div>
            </div>
        </div>
    </section>
</div>",
                    Css = @".page-wrapper {
    /* hide image sliding in from right offpage */
    overflow-x: hidden;
}

.image-container {
    margin-top: 48px;
}

@media (min-width: 1200px) {
    .image-container {
        margin-top: 0;
    }
}

.slb-detail-item ::ng-deep a {
    color: rgb(172, 0, 215);
}

.add-to-tenant-subsection h1 {
    display: none;
}

.slb-system-requirements-table ::ng-deep table {
    border-top: solid 1px rgba(0, 103, 184, 0.75);
}

.slb-system-requirements-table ::ng-deep table tr {
    border-bottom: solid 1px rgba(0, 103, 184, 0.75);
}

.slb-system-requirements-table ::ng-deep table td {
    padding: 0.75rem 0;
}
"
                }
            );

            SaveChanges();

            var metaDataObject1 = new Models.UI.MetaData
            {
                DisplayInfo = new Models.UI.DisplayInfo
                {
                    PageTemplateId = "template-1",
                    SiteDescriptor = "News, Lists, More",
                    PreviewImages = new List<Models.UI.PreviewImage>
                    {
                        new Models.UI.PreviewImage { Type = "fullpage", AltText = "", Url = "../../assets/images/HubConnected.png" },
                        new Models.UI.PreviewImage { Type = "cardpreview", AltText = "", Url = "../../assets/images/HubConnected.png" }
                    },
                    DetailItemCategories = new List<Models.UI.DetailItemCategory>
                    {
                        new Models.UI.DetailItemCategory
                        {
                            Name = "Webparts",
                            Items = new List<Models.UI.DetailItem>
                            {
                                new Models.UI.DetailItem{
                                    Name = "Events",
                                    Description = "Compact layout",
                                    Url = "https://support.office.com/en-us/article/use-the-events-web-part-5fe4da93-5fa9-4695-b1ee-b0ae4c981909"
                                },
                                new Models.UI.DetailItem {
                                    Name = "Group Calendar",
                                    Description = "Office 365 group calendar",
                                    Url = "https://support.office.com/en-us/article/use-the-group-calendar-web-part-eaf3c04d-5699-48cb-8b5e-3caa887d51ce"
                                },
                                new Models.UI.DetailItem {
                                    Name = "Highlighted Content",
                                    Description = "Sourced library, card view",
                                    Url = "https://support.office.com/en-us/article/use-the-highlighted-content-web-part-e34199b0-ff1a-47fb-8f4d-dbcaed329efd"
                                },
                                new Models.UI.DetailItem  {
                                    Name = "Instagram",
                                    BadgeText = "Custom Web Part"
                                },
                                new Models.UI.DetailItem   {
                                    Name = "News",
                                    Description = "Carousel, top story layouts",
                                    Url = "https://support.office.com/en-us/article/use-the-news-web-part-on-a-sharepoint-page-c2dcee50-f5d7-434b-8cb9-a7feefd9f165"
                                }
                            }
                        },
                        new Models.UI.DetailItemCategory
                        {
                            Name = "Features",
                            Items = new List<Models.UI.DetailItem>
                            {
                                new Models.UI.DetailItem {
                                    Name = "Header background"
                                },
                                new Models.UI.DetailItem {
                                    Name = "Section background"
                                },
                                new Models.UI.DetailItem {
                                    Name = "Footer",
                                    Url = "https://support.office.com/en-us/article/change-the-look-of-your-sharepoint-site-06bbadc3-6b04-4a60-9d14-894f6a170818"
                                },
                                new Models.UI.DetailItem {
                                    Name = "Custom logo",
                                    BadgeText = "Coming Soon"
                                }
                            }
                        },
                        new Models.UI.DetailItemCategory
                        {
                            Name = "Site Content",
                            Items = new List<Models.UI.DetailItem>
                            {
                                new Models.UI.DetailItem { Name = "New site collection using communication site template, unless applied on top of existing site"},
                                new Models.UI.DetailItem { Name = "Custom welcome page"},
                                new Models.UI.DetailItem { Name = "7 news articles with example content"},
                                new Models.UI.DetailItem { Name = "Sample image content used in the template"}
                            }
                        },
                        new Models.UI.DetailItemCategory
                        {
                            Name = "Resources",
                            Items = new List<Models.UI.DetailItem>
                            {
                                new Models.UI.DetailItem { Name = "Walkthrough", Description = "A walkthrough of the site" }
                            }
                        },
                        new Models.UI.DetailItemCategory
                        {
                            Name = "Extras",
                            Items = new List<Models.UI.DetailItem>
                            {
                                new Models.UI.DetailItem { Name = "Just an extra item for no reason."}
                            }
                        }
                    },
                    SystemRequirements = new List<Models.UI.SystemRequirement>
                    {
                        new Models.UI.SystemRequirement { Name = "OS", Value = "SharePoint" },
                        new Models.UI.SystemRequirement { Name = "Permission required", Value = "Tenant Admin" },
                        new Models.UI.SystemRequirement { Name = "Site language", Value = "English" },
                    }
                }
            };

            var metaDataObject2 = new Models.UI.MetaData
            {
                DisplayInfo = new Models.UI.DisplayInfo
                {
                    PageTemplateId = "template-2",
                    DetailItemCategories = metaDataObject1.DisplayInfo.DetailItemCategories,
                    PreviewImages = new List<Models.UI.PreviewImage>
                    {
                        new Models.UI.PreviewImage { AltText = "Giving Site Preview", Type = "cardpreview", Url = "../../assets/images/Give@Site.png"},
                        new Models.UI.PreviewImage { AltText = "Giving Site Layout", Type = "fullpage", Url = "../../assets/images/Give@Site.png"}
                    },
                    SystemRequirements = metaDataObject1.DisplayInfo.SystemRequirements
                }
            };

            // do the thing
            Packages.AddRange(new Package
            {
                Id = Guid.Parse("c715eb26-3b70-48b6-b4cf-a18aea24bd10"),
                Visible = true,
                DisplayName = "Contoso Enterprise Landing",
                Abstract = "Put the <b>power</b> of your content, people and information in easy reach of everyone in your organization. Use a distributed news system and a variety of web parts to highlight what's happening in your organization as well as events and social conversations.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = "",
                PropertiesMetadata = metaDataObject1
            },
            new Package
            {
                Id = Guid.Parse("4efc407e-7820-400b-92f7-1b83ed32443d"),
                Visible = true,
                DisplayName = "Contoso Drone Landing",
                Abstract = "See how you can setup a welcome site using the communication site template and webparts.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            },
            new Package
            {
                Id = Guid.Parse("8d4bce19-cee0-4a81-9e06-048d60dd1eda"),
                Visible = true,
                DisplayName = "Leadership connection",
                Abstract = "Communicate leadership's vision with a dedicated homepage offering content in multiple formats. Use Hero webparts to curate key topics and video to bring vision messages directly from leadership.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            },
            new Package
            {
                Id = Guid.Parse("f27a6142-7787-4715-a81b-c08f63a8f9c1"),
                Visible = true,
                DisplayName = "Team Site",
                Abstract = "This template is an example of enabling collaboration at a team level.  It has many parts including news, files, and team calendar.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            },
            new Package
            {
                Id = Guid.Parse("1cf1d7d2-d5c8-4c79-813e-59025b0e208b"),
                Visible = true,
                DisplayName = "Giving campaign site",
                Abstract = "Create a strong call to action for everyone in your organization. Use of images and clear message make this page a resource in a giving campaign, or other focused goal that requires orgnaization wide participation.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = "",
                PropertiesMetadata = metaDataObject2
            },
            new Package
            {
                Id = Guid.Parse("dab9ebf5-8460-46f4-b65a-e5b692c30963"),
                Visible = true,
                DisplayName = "Workshop site",
                Abstract = "Create a focused training site on a single page that doesn't distract by leading users away with off-page navigation. This example also uses a Bing maps web part to provide up-to-date location information.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            },
            new Package
            {
                Id = Guid.Parse("190f3fd5-df92-4ec4-8e5e-9012d8c38ea1"),
                Visible = true,
                DisplayName = "SharePoint success site",
                Abstract = "This template shows how a generic workshop centric site could look like in SharePoint Online.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            },
            new Package
            {
                Id = Guid.Parse("e538154c-8155-4171-8bdb-f9390458be3d"),
                Visible = true,
                DisplayName = "Marketing Landing",
                Abstract = "This template is designed to demonstrate how a typical marketing site could look like in the SharePoint Online. It has a relatively simple structure with some example pages and content to get you started on updating the template based on your specific requirements.",
                Description = "",
                Version = "",
                ImagePreviewUrl = "",
                PackageUrl = ""
            });

            SaveChanges();

            PackagesPlatforms.AddRange(
                new PackagePlatform
                {
                    PackageId = Guid.Parse("c715eb26-3b70-48b6-b4cf-a18aea24bd10"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("4efc407e-7820-400b-92f7-1b83ed32443d"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("8d4bce19-cee0-4a81-9e06-048d60dd1eda"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("f27a6142-7787-4715-a81b-c08f63a8f9c1"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("1cf1d7d2-d5c8-4c79-813e-59025b0e208b"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("dab9ebf5-8460-46f4-b65a-e5b692c30963"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("190f3fd5-df92-4ec4-8e5e-9012d8c38ea1"),
                    PlatformId = "LOOKBOOK"
                },
                new PackagePlatform
                {
                    PackageId = Guid.Parse("e538154c-8155-4171-8bdb-f9390458be3d"),
                    PlatformId = "LOOKBOOK"
                }
            );

            PackagesCategories.AddRange(
                new PackageCategory
                {
                    PackageId = Guid.Parse("c715eb26-3b70-48b6-b4cf-a18aea24bd10"),
                    CategoryId = "COMMUNITY"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("4efc407e-7820-400b-92f7-1b83ed32443d"),
                    CategoryId = "DEPARTMENT"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("8d4bce19-cee0-4a81-9e06-048d60dd1eda"),
                    CategoryId = "TEAM"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("f27a6142-7787-4715-a81b-c08f63a8f9c1"),
                    CategoryId = "DEPARTMENT"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("1cf1d7d2-d5c8-4c79-813e-59025b0e208b"),
                    CategoryId = "ORGANIZATION"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("dab9ebf5-8460-46f4-b65a-e5b692c30963"),
                    CategoryId = "ORGANIZATION"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("190f3fd5-df92-4ec4-8e5e-9012d8c38ea1"),
                    CategoryId = "ORGANIZATION"
                },
                new PackageCategory
                {
                    PackageId = Guid.Parse("e538154c-8155-4171-8bdb-f9390458be3d"),
                    CategoryId = "ORGANIZATION"
                }
                );

            SaveChanges();
        }
    }
}
