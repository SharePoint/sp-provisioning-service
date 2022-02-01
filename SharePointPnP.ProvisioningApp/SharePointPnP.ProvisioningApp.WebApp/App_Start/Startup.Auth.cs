//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using SharePointPnP.ProvisioningApp.WebApp.Controllers;
using SharePointPnP.ProvisioningApp.Infrastructure.Security;
using SharePointPnP.ProvisioningApp.WebApp.Utils;
using Microsoft.Owin.Host.SystemWeb;

namespace SharePointPnP.ProvisioningApp.WebApp
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
            String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

             app.UseCookieAuthentication(new CookieAuthenticationOptions {
                CookiePath = $"/{provisioningScope}" ?? "/"
            });

            app.UseOAuth2CodeRedeemer(
                new OAuth2CodeRedeemerOptions
                {
                    ClientId = AuthenticationConfig.ClientId,
                    ClientSecret = AuthenticationConfig.ClientSecret,
                    RedirectUri = AuthenticationConfig.RedirectUri
                });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    Authority = AuthenticationConfig.Authority,
                    ClientId = AuthenticationConfig.ClientId,
                    RedirectUri = AuthenticationConfig.RedirectUri,
                    PostLogoutRedirectUri = AuthenticationConfig.RedirectUri,
                    Scope = $"{AuthenticationConfig.BasicSignInScopes} .default",
                    TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        //SecurityTokenValidated = (context) => 
                        //{
                        //    return Task.FromResult(0);
                        //},
                        AuthorizationCodeReceived = async (context) =>
                        {
                            // Upon successful sign in, get the access token & cache it using MSAL
                            IConfidentialClientApplication clientApp = MsalAppBuilder.BuildConfidentialClientApplication();
                            AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(AuthenticationConfig.GetGraphScopes(), context.Code).ExecuteAsync();
                        },
                        AuthenticationFailed = (context) =>
                        {
                            var httpContext = (HttpContextBase)context.OwinContext.Environment["System.Web.HttpContextBase"];

                            var routeData = new RouteData();
                            routeData.Values["controller"] = "Home";
                            routeData.Values["action"] = "Error";
                            routeData.Values["exception"] = context.Exception;

                            IController errorController = DependencyResolver.Current.GetService<HomeController>();
                            var requestContext = new RequestContext(httpContext, routeData);
                            errorController.Execute(requestContext);

                            // context.OwinContext.Response.Redirect("/Home/Error");
                            context.HandleResponse(); // Suppress the exception
                            //context.OwinContext.Response.Write(context.Exception.ToString());
                            return Task.FromResult(0);
                        }
                    },
                    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/samesite/owin-samesite
                    CookieManager = new SameSiteCookieManager(
                                     new SystemWebCookieManager())
                });
        }
    }
}
