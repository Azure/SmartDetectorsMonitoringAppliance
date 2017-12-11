namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System;
    using System.Collections.Generic;
    using ApplicationInsights.Extensibility;
    using ApplicationInsights.WindowsServer.TelemetryChannel;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using WebJobs.Host;

    /// <summary>
    /// A class that exposes methods that help create a tracer
    /// </summary>
    public static class TracerFactory
    {
        private static bool telemetryConigurationWasSet = false;
        private static TelemetryConfiguration secondaryTelemetryConfiguration;

        /// <summary>
        /// Create an instance of the tracer
        /// </summary>
        /// <param name="logger">Optional web jobs tracer</param>
        /// <param name="traceToConsole">Flag denoting if we should trace to console</param>
        /// <returns>An instance of <see cref="ITracer"/></returns>
        public static ITracer Create(TraceWriter logger = null, bool traceToConsole = false)
        {
            // Creates the aggregated tracer
            return new AggregatedTracer(GetTracersList(logger, traceToConsole));
        }

        /// <summary>
        /// Initializes static members of the <see cref="TracerFactory"/> class.
        /// </summary>
        private static void SetupTelemetryConfiguration()
        {
            if (!telemetryConigurationWasSet)
            {
                lock (typeof(TracerFactory))
                {
                    if (!telemetryConigurationWasSet)
                    {
                        // Get the main Ikey
                        TelemetryConfiguration.Active.InstrumentationKey = ConfigurationReader.ReadConfig("TelemetryInstrumentationKey", true);
                        TelemetryConfiguration.Active.TelemetryChannel.EndpointAddress = ConfigurationReader.ReadConfig("TelemetryEndpoint", false);

                        // Create secondary telemetry configurations if exists
                        secondaryTelemetryConfiguration = CreateAdditionalTelemetryConfiguration("SecondaryTelemetryInstrumentationKey", "SecondaryTelemetryEndpoint");

                        telemetryConigurationWasSet = true;
                    }
                }
            }
        }

        /// <summary>
        /// Prod\INT monitoring are performed in AIMON pipeline
        /// To make sure our completeness calculations are exact - we also trace to Prod pipeline, which is more reliable.
        /// </summary>
        /// <param name="instrumentationKeySettingName">The name of the instrumentation key field in the settings file</param>
        /// <param name="endPointSettingName">The name of the telemetry endpoint field in configuration file</param>
        /// <returns>Telemetry configuration</returns>
        private static TelemetryConfiguration CreateAdditionalTelemetryConfiguration(string instrumentationKeySettingName, string endPointSettingName)
        {
            TelemetryConfiguration additionalTelemetryConfiguration = null;
            string additionalIkey = ConfigurationReader.ReadConfig(instrumentationKeySettingName, required: false);
            string additionalTelemetryEndpoint = ConfigurationReader.ReadConfig(endPointSettingName, required: false);
            if (!string.IsNullOrWhiteSpace(additionalIkey) && !string.IsNullOrWhiteSpace(additionalTelemetryEndpoint))
            {
                additionalTelemetryConfiguration = CreateServerTelemetryConfiguration(additionalIkey, additionalTelemetryEndpoint);
            }

            return additionalTelemetryConfiguration;
        }

        /// <summary>
        /// Get a list of <see cref="ITracer "/> objects for creating an aggregated tracer
        /// </summary>
        /// <param name="logger">Optional web jobs tracer</param>
        /// <param name="traceToConsole">Flag denoting if we should trace to console</param>
        /// <returns>A list of <see cref="ITracer "/></returns>
        private static List<ITracer> GetTracersList(TraceWriter logger = null, bool traceToConsole = false)
        {
            string sessionId = Guid.NewGuid().ToString();
            List<ITracer> tracers = new List<ITracer>();

            if (!AzureFunctionEnvironment.IsLocalEnvironment)
            {
                SetupTelemetryConfiguration();
                tracers.Add(new ApplicationInsightsTracer(sessionId, TelemetryConfiguration.Active));

                if (secondaryTelemetryConfiguration != null)
                {
                    tracers.Add(new ApplicationInsightsTracer(sessionId, secondaryTelemetryConfiguration));
                }
            }

            if (traceToConsole)
            {
                tracers.Add(new ConsoleTracer(sessionId));
            }

            if (logger != null)
            {
                tracers.Add(new WebJobTracer(sessionId, logger));
            }

            return tracers;
        }

        /// <summary>
        /// Creates a <see cref="TelemetryConfiguration"/> object with <see cref="ServerTelemetryChannel"/>, 
        /// using the requested iKey and endpoint  
        /// </summary>
        /// <param name="instrumentationKey">Channel's instrumentation key</param>
        /// <param name="telemetryEndpoint">Channel's endpoint</param>
        /// <returns>A <see cref="TelemetryConfiguration"/> object with <see cref="ServerTelemetryChannel"/>, using the requested iKey and endpoint</returns>
        private static TelemetryConfiguration CreateServerTelemetryConfiguration(string instrumentationKey, string telemetryEndpoint)
        {
            TelemetryConfiguration telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.InstrumentationKey = instrumentationKey;
            telemetryConfig.TelemetryChannel = new ServerTelemetryChannel
            {
                EndpointAddress = telemetryEndpoint
            };

            ((ServerTelemetryChannel)telemetryConfig.TelemetryChannel).Initialize(telemetryConfig);
            return telemetryConfig;
        }
    }
}