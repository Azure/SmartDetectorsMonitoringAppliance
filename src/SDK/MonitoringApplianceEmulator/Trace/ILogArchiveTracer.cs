//-----------------------------------------------------------------------
// <copyright file="ILogArchiveTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;

    /// <summary>
    /// An interface for the Log Archive tracer - which is basically a disposable tracer
    /// </summary>
    public interface ILogArchiveTracer : ITracer, IDisposable
    {
    }
}
