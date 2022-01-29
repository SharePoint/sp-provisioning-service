//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using SharePointPnP.ProvisioningApp.Infrastructure.Security;

namespace SharePointPnP.ProvisioningApp.WebApi
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = "common", // To support multi-tenant
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = AuthenticationConfig.Audience,
                        ValidateIssuer = false, // To support multi-tenant
                    }
                });
        }
    }
}