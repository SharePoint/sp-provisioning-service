using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharePoint.Portal.Web.Telemetry
{
    /// <summary>
    // Filters out dev server requests to sockjs
    /// </summary>
    public class SockJsFilterTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public SockJsFilterTelemetryProcessor(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry reqItem)
            {
                if (reqItem.Name.Contains("/sockjs-node/"))
                {
                    return;
                }

            }

            if (item is DependencyTelemetry depItem)
            {
                if (depItem.Name.Contains("/sockjs-node/"))
                {
                    return;
                }
            }

            Next.Process(item);
        }
    }
}
