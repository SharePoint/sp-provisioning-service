//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// Defines a custom exception for concurrent provisioning events
    /// </summary>
    public class ConcurrentProvisioningException: ApplicationException
    {
        public ConcurrentProvisioningException(String message): base(message)
        {
        }

        public ConcurrentProvisioningException(String message, Exception innerException): base(message, innerException)
        {
        }

        public ConcurrentProvisioningException(): base()
        {
        }
    }
}
