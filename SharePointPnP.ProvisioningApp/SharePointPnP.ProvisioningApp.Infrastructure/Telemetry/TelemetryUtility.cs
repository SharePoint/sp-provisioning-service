﻿//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace SharePointPnP.ProvisioningApp.Infrastructure.Telemetry
{
    public class TelemetryUtility
    {
        private readonly TelemetryClient telemetryClient;

        #region Construction
        /// <summary>
        /// Instantiates the telemetry client
        /// </summary>
        public TelemetryUtility(Action<string> log)
        {
            try
            {
                var instrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];

                if (!String.IsNullOrEmpty(instrumentationKey))
                {
                    this.telemetryClient = new TelemetryClient
                    {
                        InstrumentationKey = instrumentationKey
                    };

                    // Setting this is needed to make metric tracking work
                    TelemetryConfiguration.Active.InstrumentationKey = this.telemetryClient.InstrumentationKey;

                    this.telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                    this.telemetryClient.Context.Cloud.RoleInstance = "SharePointPnPProvisioningService";
                    this.telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                    var coreAssembly = Assembly.GetExecutingAssembly();
                    this.telemetryClient.Context.GlobalProperties.Add("Version", ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.ToString());

                    log?.Invoke("Telemetry setup done");
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient = null;
                log?.Invoke($"Telemetry setup failed: {ex.Message}. Continuing without telemetry");
            }
        }
        #endregion

        public void LogEvent(String eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (this.telemetryClient == null)
            {
                return;
            }

            try
            {
                // Prepare event data
                properties = properties ?? new Dictionary<string, string>();
                metrics = metrics ?? new Dictionary<string, double>();

                properties["TestingEnvironment"] = ProvisioningAppManager.IsTestingEnvironment.ToString();

                // Log the event
                this.telemetryClient.TrackEvent(eventName, properties, metrics);
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        public void LogException(Exception ex, string location, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (this.telemetryClient == null || ex == null)
            {
                return;
            }

            try
            {
                // Prepare event data
                properties = properties ?? new Dictionary<string, string>();
                metrics = metrics ?? new Dictionary<string, double>();

                properties["TestingEnvironment"] = ProvisioningAppManager.IsTestingEnvironment.ToString();
                properties["DetailedException"] = ex.ToDetailedString();

                if (!string.IsNullOrEmpty(location))
                {
                    properties.Add("Location", location);
                }

                // Log the exception
                this.telemetryClient.TrackException(ex, properties, metrics);
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        /// <summary>
        /// Ensure telemetry data is send to server
        /// </summary>
        public void Flush()
        {
            try
            {
                if (this.telemetryClient != null)
                {
                    // before exit, flush the remaining data
                    this.telemetryClient.Flush();

                    // flush is not blocking so wait a bit
                    Task.Delay(500).Wait();
                }
            }
            catch
            {
                // Eat all exceptions, we don't want this code to block anything
            }
        }
    }
}
