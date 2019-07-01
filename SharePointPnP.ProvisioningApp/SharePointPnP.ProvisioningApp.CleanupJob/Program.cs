using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharePointPnP.ProvisioningApp.Infrastructure;

namespace SharePointPnP.ProvisioningApp.CleanupJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        static void Main()
        {
            Task.Run(async () =>
            {
                // Get the reference date for removal of keys (expired at least 2 hours ago)
                var referenceDateTime = DateTime.Now.AddHours(-2);

                // This job cleans up the expired keys from the Azure Key Vault
                var vault = new KeyVaultService();

                // Get all the keys stored in Key Vault
                var allKeys = await vault.ListKeysAsync();

                // Check for expired keys
                foreach (var key in allKeys)
                {
                    var currentKey = await vault.GetFullKeyAsync(key);
                    if (currentKey.Attributes.Expires <= referenceDateTime)
                    {
                        // The key is expired, let's remove it
                        await vault.RemoveKeyAsync(key);

                        // Log the action
                        Console.WriteLine($"Deleted key {key} that expired on {currentKey.Attributes.Expires}");
                    }
                    else
                    {
                        Console.WriteLine($"Key {key} is not yet expired");
                    }

                    // Delay to avoid throttling
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }).GetAwaiter().GetResult();
        }
    }
}
