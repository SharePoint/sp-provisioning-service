using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using SharePointPnP.ProvisioningApp.DomainModel;

namespace SharePointPnP.ProvisioningApp.WebApi.Controllers
{
    [Authorize]
    public class CategoriesController : ODataController
    {
        ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        [EnableQuery]
        public IQueryable<Category> Get()
        {
            return dbContext.Categories;
        }

        [EnableQuery]
        public SingleResult<Category> Get([FromODataUri] String key)
        {
            IQueryable<Category> result = dbContext.Categories.Where(c => c.Id == key);
            return SingleResult.Create(result);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}