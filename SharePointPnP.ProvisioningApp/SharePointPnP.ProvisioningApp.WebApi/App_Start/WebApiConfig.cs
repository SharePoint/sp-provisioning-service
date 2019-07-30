//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using SharePointPnP.ProvisioningApp.DomainModel;

namespace SharePointPnP.ProvisioningApp.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Package>("Packages");
            var packages = builder.EntityType<Package>();
            packages.Count().Filter().Select().OrderBy();

            builder.EntitySet<Category>("Categories");
            var categories = builder.EntityType<Category>();
            categories.Count().Filter().Select().OrderBy();

            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());
        }
    }
}
