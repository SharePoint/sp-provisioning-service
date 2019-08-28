//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning
{
    /// <summary>
    /// Defines a webhook provided in a provisioning request
    /// </summary>
    public class ProvisioningWebhook
    {
        /// <summary>
        /// The URL of the Webhook
        /// </summary>
        public String Url { get; set; }

        /// <summary>
        /// The HTTP Method for the Webhook
        /// </summary>
        public WebhookMethod Method { get; set; }

        /// <summary>
        /// The parameters for the Webhook
        /// </summary>
        public Dictionary<String, String> Parameters { get; set; }
    }

    /// <summary>
    /// Defines the available HTTP Methods for the Webhooks
    /// </summary>
    public enum WebhookMethod
    {
        /// <summary>
        /// HTTP GET
        /// </summary>
        GET,
        /// <summary>
        /// HTTP POST
        /// </summary>
        POST,
    }
}