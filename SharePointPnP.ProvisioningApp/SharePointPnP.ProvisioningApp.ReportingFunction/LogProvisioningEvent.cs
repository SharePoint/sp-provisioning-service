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
    public static class LogProvisioningEvent
    {
        [FunctionName("LogProvisioningEvent")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Reporting function triggered.");

            Exception exception = null;

            try
            {
                // Read request data
                string requestBody = await req.Content.ReadAsStringAsync();
                ProvisioningEvent provisioningEvent = JsonConvert.DeserializeObject<ProvisioningEvent>(requestBody);

                var reportingConnectionString = ConfigurationManager.ConnectionStrings["PnPProvisioningReportingDBContext"].ConnectionString;

                using (var connection = new SqlConnection(reportingConnectionString))
                {
                    using (var command = new SqlCommand("InsertProvisioningEvent", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (provisioningEvent.EventId != Guid.Empty)
                        {
                            command.Parameters.Add("@EventId", SqlDbType.UniqueIdentifier).Value = provisioningEvent.EventId;
                        }
                        command.Parameters.Add("@EventStartDateTime", SqlDbType.DateTime).Value = provisioningEvent.EventStartDateTime;
                        command.Parameters.Add("@EventEndDateTime", SqlDbType.DateTime).Value = provisioningEvent.EventEndDateTime;
                        command.Parameters.Add("@EventOutcome", SqlDbType.Int).Value = provisioningEvent.EventOutcome;
                        command.Parameters.Add("@EventDetails", SqlDbType.NVarChar, -1).Value = provisioningEvent.EventDetails;
                        command.Parameters.Add("@EventFromProduction", SqlDbType.Bit).Value = provisioningEvent.EventFromProduction;
                        command.Parameters.Add("@TemplateId", SqlDbType.UniqueIdentifier).Value = provisioningEvent.TemplateId;
                        command.Parameters.Add("@TemplateDisplayName", SqlDbType.NVarChar, 200).Value = provisioningEvent.TemplateDisplayName;

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
    /// Defines a Provisioning Event to be logged
    /// </summary>
    public class ProvisioningEvent
    {
        /// <summary>
        /// The ID of the Provisioning Action
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The Start Date and Time of the Provisioning Action
        /// </summary>
        public DateTime EventStartDateTime { get; set; }

        /// <summary>
        /// The End Date and Time of the Provisioning Action
        /// </summary>
        public DateTime EventEndDateTime { get; set; }

        /// <summary>
        /// The Outcome of the Provisioning Action
        /// </summary>
        public EventOutcomes EventOutcome { get; set; }

        /// <summary>
        /// Any additional Details about the Provisioning Action
        /// </summary>
        public String EventDetails { get; set; }

        /// <summary>
        /// Declares whether the event is about a Provisioning Action running in Production or in Test environment
        /// </summary>
        public Boolean EventFromProduction { get; set; }

        /// <summary>
        /// The ID of the target Provisioning Template package, if any
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// The Display Name of the target Provisioning Template package, if any
        /// </summary>
        public String TemplateDisplayName { get; set; }
    }

    /// <summary>
    /// Defines the possible outcomes of a provisioning job
    /// </summary>
    public enum EventOutcomes
    {
        /// <summary>
        /// The job is running
        /// </summary>
        Running,
        /// <summary>
        /// The job succeeded
        /// </summary>
        Succeeded,
        /// <summary>
        /// The job failed
        /// </summary>
        Failed,
    }
}
