//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.WebJobServiceBus
{ 
    public class KnownExceptions
    {
        [JsonProperty("knownExceptions")]
        public List<KnownException> Exceptions { get; set; }
    }

    public class KnownException
    {
        public String ExceptionType { get; set; }

        public String MatchingText { get; set; }

        public String FriendlyMessageResourceKey { get; set; }
    }
}
