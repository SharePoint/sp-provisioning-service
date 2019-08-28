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
    public class CategoriesController : ODataController
    {
        readonly ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        [EnableQuery]
        public IQueryable<Category> Get()
        {
            // Manage Authorization Checks
            ApiSecurityHelper.CheckRequestAuthorization(true);

            return dbContext.Categories;
        }

        [EnableQuery]
        public SingleResult<Category> Get([FromODataUri] String key)
        {
            // Manage Authorization Checks
            ApiSecurityHelper.CheckRequestAuthorization(true);

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