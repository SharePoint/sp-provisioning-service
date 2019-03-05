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
                model.Packages = context.Packages.Include("Categories").ToList();
            }
            else
            {
                // Show not-preview packages in the production environment
                model.Packages = context.Packages.Include("Categories").Where(p => p.Preview == false).ToList();
            }

            // Get the service description content
            var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ServiceDescription.md");

            if (contentPage != null)
            {
                model.ServiceDescription = contentPage.Content;
            }

            // Get the categories
            model.Categories = context.Categories.ToDictionary(c => c.Id, c => c.DisplayName);

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


    }
}