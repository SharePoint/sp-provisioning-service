//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.WebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new JobHostConfiguration();
            // configuration.Queues.BatchSize = 1;
            configuration.Queues.MaxDequeueCount = 1;

            var host = new JobHost(configuration);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
