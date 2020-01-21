//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace SharePointPnP.ProvisioningApp.Infrastructure.QueueUtilities
{
    /// <summary>
    /// Utility class to enqueue messages in the Azure Service Bus Queue
    /// </summary>
    public static class ServiceBusQueueUtility
    {
        public async static Task EnqueueMessageAsync(String connectionString, String queueName, Object data)
        {
            // Prepare the queue variable
            QueueClient queueClient = null;

            try
            {
                // Open the queue
                queueClient = new QueueClient(connectionString, queueName);

                // Prepare the message
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));

                // Send the message to the queue.
                await queueClient.SendAsync(message);
            }
            finally
            {
                // Release the queue in case of need
                if (queueClient != null && !queueClient.IsClosedOrClosing)
                {
                    // Close the queue
                    await queueClient.CloseAsync();
                }
            }
        }
    }
}