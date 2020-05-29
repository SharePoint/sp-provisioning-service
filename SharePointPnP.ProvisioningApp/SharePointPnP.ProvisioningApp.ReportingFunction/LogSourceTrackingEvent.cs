//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace SharePointPnP.ProvisioningApp.ReportingFunction
{
    public static class LogSourceTrackingEvent
    {
        [FunctionName("LogSourceTrackingEvent")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Source Tracking function triggered.");

            Exception exception = null;

            try
            {
                // Read request data
                string requestBody = await req.Content.ReadAsStringAsync();
                SourceTrackingEvent sourceTrackingEvent = JsonConvert.DeserializeObject<SourceTrackingEvent>(requestBody);

                var reportingConnectionString = ConfigurationManager.ConnectionStrings["PnPProvisioningReportingDBContext"].ConnectionString;

                using (var connection = new SqlConnection(reportingConnectionString))
                {
                    using (var command = new SqlCommand("InsertSourceTracking", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@SourceId", SqlDbType.NVarChar, 50).Value = sourceTrackingEvent.SourceId;
                        command.Parameters.Add("@SourceTrackingDateTime", SqlDbType.DateTime).Value = sourceTrackingEvent.SourceTrackingDateTime.HasValue ? sourceTrackingEvent.SourceTrackingDateTime : DateTime.Now;
                        command.Parameters.Add("@SourceTrackingAction", SqlDbType.TinyInt).Value = sourceTrackingEvent.SourceTrackingAction;
                        command.Parameters.Add("@SourceTrackingUrl", SqlDbType.NVarChar, 500).Value = sourceTrackingEvent.SourceTrackingUrl;
                        command.Parameters.Add("@SourceTrackingFromProduction", SqlDbType.Bit).Value = sourceTrackingEvent.SourceTrackingFromProduction;
                        command.Parameters.Add("@TemplateId", SqlDbType.UniqueIdentifier).Value = sourceTrackingEvent.TemplateId;
                        command.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = sourceTrackingEvent.TenantId;
                        command.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier).Value = sourceTrackingEvent.SiteId;

                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return (exception == null) ? req.CreateResponse(HttpStatusCode.Accepted) :
                req.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
        }
    }

    /// <summary>
    /// Defines a Source Tracking Event to be logged
    /// </summary>
    public class SourceTrackingEvent
    {
        /// <summary>
        /// The unique ID of the tracking source
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// The Start Date and Time of the Provisioning Action
        /// </summary>
        public DateTime? SourceTrackingDateTime { get; set; }

        /// <summary>
        /// The action to keep track of
        /// </summary>
        public SourceTrackingAction SourceTrackingAction { get; set; }

        /// <summary>
        /// The URL of the page, if the action is PageView
        /// </summary>
        public string SourceTrackingUrl { get; set; }

        /// <summary>
        /// Declares whether the event is about a Provisioning Action running in Production or in Test environment
        /// </summary>
        public Boolean SourceTrackingFromProduction { get; set; }

        /// <summary>
        /// The ID of the target Provisioning Template package, if any
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// The ID of the target tenant, if any
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The ID of the target site provisioned, if any
        /// </summary>
        public Guid SiteId { get; set; }
    }

    /// <summary>
    /// Defines the possible outcomes of a provisioning job
    /// </summary>
    public enum SourceTrackingAction
    {
        /// <summary>
        /// The view of a page
        /// </summary>
        PageView,
        /// <summary>
        /// The provisioning of a template
        /// </summary>
        Provisioning,
        /// <summary>
        /// The provisioning of a template is now complete
        /// </summary>
        Provisioned,
    }
}
