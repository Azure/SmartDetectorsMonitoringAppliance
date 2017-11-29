namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class representing an aggregated operation handlers.
    /// </summary>
    internal sealed class AggregateOperationHandler : ITelemetryOperationHandler
    {
        private readonly List<ITelemetryOperationHandler> _operationHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateOperationHandler"/> class.
        /// </summary>
        /// <param name="operationHandlers">The aggregated request operations</param>
        public AggregateOperationHandler(IEnumerable<ITelemetryOperationHandler> operationHandlers)
        {
            Diagnostics.EnsureArgumentNotNull(() => operationHandlers);

            _operationHandlers = operationHandlers.Where(a => a != null).ToList();
        }

        #region Implementation of ITelemetryOperationHandler

        /// <summary>
        /// Initiate an operation - Do nothing.
        /// </summary>
        /// <param name="operationName">The operation name to be used by the tracer</param>
        public void StartOperation(string operationName)
        {
            _operationHandlers.ForEach(h => h.StartOperation(operationName));
        }

        /// <summary>
        /// Loop through all handlers and call their MarkOperationAsFailure
        /// </summary>
        public void MarkOperationAsFailure()
        {
            _operationHandlers.ForEach(h => h.MarkOperationAsFailure());
        }

        /// <summary>
        /// Sends an operation summary telemetry.
        /// </summary>
        public void DispatchOperation()
        {
            _operationHandlers.ForEach(h => h.DispatchOperation());
        }

        /// <summary>
        /// Add a custom dimension to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Dimension's name</param>
        /// <param name="value">Dimension's value</param>
        public void AddCustomDimension(string name, string value)
        {
            _operationHandlers.ForEach(h => h.AddCustomDimension(name, value));
        }

        /// <summary>
        /// Add custom dimensions to the operation summary telemetry.
        /// </summary>
        /// <param name="dimensions">Dimensions to add</param>
        public void AddCustomDimensions(IDictionary<string, string> dimensions)
        {
            _operationHandlers.ForEach(h => h.AddCustomDimensions(dimensions));
        }

        /// <summary>
        /// Add a custom measurement to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Measurement's name</param>
        /// <param name="value">Measurement's value</param>
        public void AddCustomMeasurement(string name, double value)
        {
            _operationHandlers.ForEach(h => h.AddCustomMeasurement(name, value));
        }

        /// <summary>
        /// Sets the synthetic source field for telemetry sent during the operation.
        /// </summary>
        public string SyntheticSource
        {
            set
            {
                _operationHandlers.ForEach(h => h.SyntheticSource = value);
            }
        }

        /// <summary>
        /// Sets the UserId field for telemetry sent during the operation.
        /// </summary>
        public string UserId
        {
            set
            {
                _operationHandlers.ForEach(h => h.UserId = value);
            }
        }

        #endregion
    }
}