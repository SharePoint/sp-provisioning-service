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
    // [Authorize]
    public class PackagesController : ODataController
    {
        ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        [EnableQuery]
        public IQueryable<Package> Get()
        {
            return dbContext.Packages;
        }

        [EnableQuery]
        public SingleResult<Package> Get([FromODataUri] Guid key)
        {
            IQueryable<Package> result = dbContext.Packages.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}