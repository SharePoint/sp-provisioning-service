using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Functions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                //.ConfigureAppConfiguration(config =>
                //{
                //    config
                //        .AddJsonFile("local.settings.json", true, false)
                //        .AddEnvironmentVariables()
                //        .Build();
                //})
                .Build();

            host.Run();
        }
    }
}