namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared.Trace
{
    using System;
    using System.Collections.Generic;
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility;

    /// <summary>
    /// Implementation of the <see cref="ITelemetryOperationHandler"/> interface that handles appinsights operations.
    /// </summary>
    public sealed class ApplicationInsightsRequestOperationHandler : ITelemetryOperationHandler
    {
        /// <summary>B
        /// DeepInsights telemetry client
        /// </summary>
        private TelemetryClient _telemetryClient;

        /// <summary>
        /// Request operation holder
        /// </summary>
        private IOperationHolder<RequestTelemetry> _requestTelemetryOperationHandler;

        /// <summary>
        /// Operation's predefined custom properties
        /// </summary>
        private IDictionary<string, string> _customProperties;

        /// <summary>
        /// Gets the synthetic source field for telemetry sent during the operation.
        /// </summary>
        public string SyntheticSource
        {
            set
            {
                if (_requestTelemetryOperationHandler == null)
                {
                    throw new InvalidOperationException($"Can't set SyntheticSource, before calling to {nameof(StartOperation)}");
                }

                _requestTelemetryOperationHandler.Telemetry.Context.Operation.SyntheticSource = value;
            }
        }

        /// <summary>
        /// Gets the UserId field for telemetry sent during the operation.
        /// </summary>
        public string UserId
        {
            set
            {
                if (_requestTelemetryOperationHandler == null)
                {
                    throw new InvalidOperationException($"Can't set UserId, before calling to {nameof(StartOperation)}");
                }

                _requestTelemetryOperationHandler.Telemetry.Context.User.Id = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsRequestOperationHandler"/> class.
        /// </summary>
        /// <param name="telemetryClient">Appinsights tracer used during the operation</param>
        /// <param name="customProperties">List of custom properties to be reported as part of the request</param>
        public ApplicationInsightsRequestOperationHandler(TelemetryClient telemetryClient, IDictionary<string, string> customProperties)
        {
            _telemetryClient = telemetryClient;
            _customProperties = customProperties;
        }

        /// <summary>
        /// Starts a new operation.
        /// Only one operation in parallel is supported.
        /// In order to release the operation, <see cref="DispatchOperation"/> should be called.
        /// </summary>
        /// <param name="operationName">The name of the operation, to be used as OperationName in all telemtry items along the operation</param>
        public void StartOperation(string operationName)
        {
            if (_requestTelemetryOperationHandler != null)
            {
                throw new InvalidOperationException("StartOperation was called, while another operation is still active.");
            }

            _requestTelemetryOperationHandler = _telemetryClient.StartOperation<RequestTelemetry>(operationName);
            AddCustomDimensions(_customProperties);
        }

        /// <summary>
        /// Sets the operation context properties in <paramref name="telemetry"/> based on the current operation <see cref="StartOperation"/>.
        /// In case no operation was started, the context will not be populated.
        /// </summary>
        /// <param name="telemetry">The telemetry to set properties in</param>
        public void SetTelemetryOperationContext(ITelemetry telemetry)
        {
            //No operation was started.
            if (_requestTelemetryOperationHandler == null)
            {
                return;
            }

            telemetry.Context.Operation.SyntheticSource = _requestTelemetryOperationHandler.Telemetry.Context.Operation.SyntheticSource;
            telemetry.Context.User.Id = _requestTelemetryOperationHandler.Telemetry.Context.User.Id;

            if (telemetry.Context.Cloud != null)
            {
                telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
            }
        }

        /// <summary>
        /// Sends an operation summary telemetry.
        /// </summary>
        public void DispatchOperation()
        {
            _requestTelemetryOperationHandler.Dispose();
            _requestTelemetryOperationHandler = null;
        }

        #region Telemetry functions

        /// <summary>
        /// Add a custom dimension to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Dimension's name</param>
        /// <param name="value">Dimension's value</param>
        public void AddCustomDimension(string name, string value)
        {
            _requestTelemetryOperationHandler.Telemetry.Properties[name] = value;
        }

        /// <summary>
        /// Add custom dimensions to the operation summary telemetry.
        /// </summary>
        /// <param name="dimensions">Dimensions to add</param>
        public void AddCustomDimensions(IDictionary<string, string> dimensions)
        {
            foreach (var dimension in dimensions)
            {
                AddCustomDimension(dimension.Key, dimension.Value);
            }
        }

        /// <summary>
        /// Add a custom measurement to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Measurement's name</param>
        /// <param name="value">Measurement's value</param>
        public void AddCustomMeasurement(string name, double value)
        {
            _requestTelemetryOperationHandler.Telemetry.Metrics[name] = value;
        }

        /// <summary>
        /// Mark operation as failure
        /// </summary>
        public void MarkOperationAsFailure()
        {
            _requestTelemetryOperationHandler.Telemetry.ResponseCode = "500";
        }

        #endregion
    }
}