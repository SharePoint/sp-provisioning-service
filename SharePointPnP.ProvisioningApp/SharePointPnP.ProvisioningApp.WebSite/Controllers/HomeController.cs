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
        public ActionResult CategoriesMenu()
        {
            CategoriesMenuViewModel model = new CategoriesMenuViewModel();

            // Get all the Categories together with the Packages
            ProvisioningAppDBContext context = new ProvisioningAppDBContext();
            model.Categories = context.Categories
                .Include("Packages")
                .OrderBy(c => c.Order)
                .ToList();

            return PartialView("CategoriesMenu", model);
        }
    }
}