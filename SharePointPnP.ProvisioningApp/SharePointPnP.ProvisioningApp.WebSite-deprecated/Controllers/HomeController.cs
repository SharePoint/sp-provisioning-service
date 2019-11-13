//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.WebSite.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SharePointPnP.ProvisioningApp.WebSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IndexViewModel model = new IndexViewModel();

            ProvisioningAppDBContext context = new ProvisioningAppDBContext();

            // Get the packages
            if (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]))
            {
                // Show all packages in the test environment
                model.Packages = context.Packages.Include("Categories").Include("TargetPlatforms").Where(p => p.Visible == true).ToList();
            }
            else
            {
                // Show not-preview packages in the production environment
                model.Packages = context.Packages.Include("Categories").Include("TargetPlatforms").Where(p => p.Preview == false && p.Visible == true).ToList();
            }

            // Get the service description content
            var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ServiceDescription.md");

            if (contentPage != null)
            {
                model.ServiceDescription = contentPage.Content;
            }

            // Get the categories
            model.Categories = context.Categories.OrderBy(c => c.Order).ToList();

            return View(model);
        }

        [Route("~/home/{packageUrl}")]
        public ActionResult DetailsByPath(String packageUrl)
        {
            ProvisioningAppDBContext context = new ProvisioningAppDBContext();
            Package targetPackage = null;

            packageUrl.Replace("-", "/");

            // Get the package
            if (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]))
            {
                // Show any package in the test environment
                targetPackage = context.Packages.FirstOrDefault(p => p.PackageUrl == packageUrl);
            }
            else
            {
                // Show not-preview packages in the production environment
                targetPackage = context.Packages.FirstOrDefault(p => p.PackageUrl == packageUrl && p.Preview == false);
            }

            if (targetPackage != null)
            {
                return (RedirectToAction("Details", targetPackage.Id));
            }
            else
            {
                throw new ApplicationException("There is no Package with the provided packageUrl!");
            }
        }

        public ActionResult Details(String packageId)
        {
            DetailsViewModel model = new DetailsViewModel();

            ProvisioningAppDBContext context = new ProvisioningAppDBContext();

            // Get the package
            if (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]))
            {
                // Show any package in the test environment
                model.Package = context.Packages.Include("Categories").FirstOrDefault(p => p.Id == new Guid(packageId));
            }
            else
            {
                // Show not-preview packages in the production environment
                model.Package = context.Packages.Include("Categories").FirstOrDefault(p => p.Id == new Guid(packageId) && p.Preview == false);
            }

            if (model.Package == null)
            {
                throw new ApplicationException("There is no Package with the provided packageId!");
            }

            return View("Details", model);
        }

        public ActionResult ContentPage(String contentPageId)
        {
            ContentPageViewModel model = new ContentPageViewModel();

            ProvisioningAppDBContext context = new ProvisioningAppDBContext();
            var targetContentPageId = $"system/pages/{contentPageId}.md";
            var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == targetContentPageId);

            if (contentPage == null)
            {
                throw new ApplicationException("There is no Content Page with the provided contentPageId!");
            }

            model.Content = contentPage.Content;

            return View("ContentPage", model);
        }

        [HttpPost]
        public ActionResult CategoriesMenu(String returnUrl = null)
        {
            CategoriesMenuViewModel model = new CategoriesMenuViewModel();

            // Let's see if we need to filter the output categories
            var slbHost = System.Configuration.ConfigurationManager.AppSettings["SPLBSiteHost"];
            var testEnvironment = Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]);

            string targetPlatform = null;

            if (!String.IsNullOrEmpty(returnUrl) &&
                !String.IsNullOrEmpty(slbHost) &&
                returnUrl.Contains(slbHost))
            {
                model.BaseLinksUrl = returnUrl.Substring(0, returnUrl.LastIndexOf(@"/") + 1);
                targetPlatform = "LOOKBOOK";
            }
            else
            {
                model.BaseLinksUrl = String.Empty;
                targetPlatform = "SPPNP";
            }

            // Get all the Categories together with the Packages
            ProvisioningAppDBContext context = new ProvisioningAppDBContext();

            var tempCategories = context.Categories
                .AsNoTracking()
                .Include("Packages")
                .Where(c => c.Packages.Any(
                    p => p.Visible &&
                    (testEnvironment || !p.Preview) &&
                    p.TargetPlatforms.Any(pf => pf.Id == targetPlatform)
                ))
                .OrderBy(c => c.Order)
                .ToList();

            model.Categories = tempCategories;

            return PartialView("CategoriesMenu", model);
        }
    }
}