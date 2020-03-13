//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.QueueUtilities
{
    /// <summary>
    /// Utility class to enqueue messages in the Azure Blob Storage Queue
    /// </summary>
    public static class BlobStorageQueueUtility
    {
        public static Task EnqueueMessageAsync(String connectionString, String queueName, Object data)
        {
            return Task.Run(() => {
                // Get a reference to the blob storage queue
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Get queue... create if does not exist.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference(queueName);
                queue.CreateIfNotExists();

                // add message to the queue
                queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(data)));
            });
        }
    }
}