namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface providing telemetry operation handling capabilities
    /// </summary>
    public interface ITelemetryOperationHandler
    {
        /// <summary>
        /// Starts a new operation.
        /// </summary>
        /// <param name="operationName">The name of the operation, to be used as OperationName in all telemetry items along the operation</param>
        void StartOperation(string operationName);

        /// <summary>
        /// Mark operation as failure
        /// </summary>
        void MarkOperationAsFailure();

        /// <summary>
        /// Sends an operation summary telemetry.
        /// </summary>
        void DispatchOperation();

        /// <summary>
        /// Add a custom dimension to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Dimension's name</param>
        /// <param name="value">Dimension's value</param>
        void AddCustomDimension(string name, string value);

        /// <summary>
        /// Add custom dimensions to the operation summary telemetry.
        /// </summary>
        /// <param name="dimensions">Dimensions to add</param>
        void AddCustomDimensions(IDictionary<string, string> dimensions);

        /// <summary>
        /// Add a custom measurement to the operation summary telemetry.
        /// </summary>
        /// <param name="name">Measurement's name</param>
        /// <param name="value">Measurement's value</param>
        void AddCustomMeasurement(string name, double value);

        /// <summary>
        /// Sets the synthetic source field for telemetry sent during the operation.
        /// </summary>
        string SyntheticSource { set; }

        /// <summary>
        /// Sets the UserId field for telemetry sent during the operation.
        /// </summary>
        string UserId { set; }
    }
}