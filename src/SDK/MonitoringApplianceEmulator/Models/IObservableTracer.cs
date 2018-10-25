//-----------------------------------------------------------------------
// <copyright file="IObservableTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Text;

    /// <summary>
    /// Interface providing tracing capabilities for observable tracer.
    /// </summary>
    public interface IObservableTracer : ITracer
    {
        /// <summary>
        /// Gets all traces
        /// </summary>
        StringBuilder Traces { get; }

        /// <summary>
        /// Clears all traces
        /// </summary>
        void Clear();
    }
}