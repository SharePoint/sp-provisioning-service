//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.WebApi.Components;

namespace SharePointPnP.ProvisioningApp.WebApi.Controllers
{
    [Authorize]
    public class PackagesController : ODataController
    {
        readonly ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        [EnableQuery]
        public IQueryable<Package> Get()
        {
            // Manage Authorization Checks
            ApiSecurityHelper.CheckRequestAuthorization(true);

            return dbContext.Packages;
        }

        [EnableQuery]
        public SingleResult<Package> Get([FromODataUri] Guid key)
        {
            // Manage Authorization Checks
            ApiSecurityHelper.CheckRequestAuthorization(true);

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