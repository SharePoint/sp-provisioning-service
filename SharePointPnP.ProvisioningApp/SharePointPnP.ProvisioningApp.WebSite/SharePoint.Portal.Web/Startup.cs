using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharePoint.Portal.Web.Data;
using SharePoint.Portal.Web.Middleware.PortalApiExceptionHandler;
using SharePoint.Portal.Web.Telemetry;
using SharePoint.Portal.Web.Business.DependencyInjection;

namespace SharePoint.Portal.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsProduction())
            {
                services.AddDbContext<PortalDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("PnPProvisioningAppDBContext")));
            }
            else
            {
                // Use in-memory database for development. 
                // The database gets recreated every time the server is started.
                services.AddDbContext<PortalDbContext>(options => options.UseInMemoryDatabase(databaseName: "lookbook"));
            }
            services.AddDependencies();

            services.AddOptions<GlobalOptions>()
                .Configure(options =>
                {
                    bool isTestEnvironment;
                    if (!bool.TryParse(Configuration["TestEnvironment"], out isTestEnvironment))
                    {
                        isTestEnvironment = false;
                    }

                    options.IsTestEnvironment = isTestEnvironment;

                    bool provisionTemplates;
                    if (!bool.TryParse(Configuration["ProvisionTemplates"], out provisionTemplates))
                    {
                        provisionTemplates = false;
                    }
                    options.ProvisionTemplates = provisionTemplates;

                    options.ProvisioningInstructionsUrl = Configuration["ProvisioningInstructionsUrl"];
                    options.ProvisioningPageBaseUrl = Configuration["ProvisioningPageBaseUrl"];
                    options.PlatformId = Configuration["PlatformId"];
                    options.TrackingUrl = Configuration["TrackingUrl"];
                    options.TelemetryUrl = Configuration["TelemetryUrl"];
                });

            // Add application insights
            //services.AddSingleton<ITelemetryInitializer, UserCorrelationTelemetryInitializer>();
            services.AddApplicationInsightsTelemetry(Configuration);
            if (Environment.IsDevelopment())
            {
                // Filter out sock-js if we are in development
                services.AddApplicationInsightsTelemetryProcessor<SockJsFilterTelemetryProcessor>();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, PortalDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                dbContext.SeedDevData();

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Disable telemetry for dev environments
            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            if (Environment.IsDevelopment())
            {
                // Comment out this line if you wish to enable application insights telemetry in dev
                configuration.DisableTelemetry = true;
            }

            app.UseApiExceptionHandler();

            // Defaults
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("https://localhost:44303/");
                }
            });
        }
    }
}
