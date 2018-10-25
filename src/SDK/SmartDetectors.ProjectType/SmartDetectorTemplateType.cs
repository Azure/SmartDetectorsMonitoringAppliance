//-----------------------------------------------------------------------
// <copyright file="SmartDetectorTemplateType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    /// <summary>
    /// This enum is used to specify the smart detector template type
    /// </summary>
    public enum SmartDetectorTemplateType
    {
        /// <summary>
        /// Empty smart detector template
        /// </summary>
        Empty,

        /// <summary>      
        /// Smart detector template for detecting alerts based on data in Log Analytics workspaces
        /// </summary>
        LogAnalytics,

        /// <summary>
        /// Smart detector template for detecting alerts based on metrics.
        /// </summary>
        Metric,

        /// <summary>
        /// Smart detector template for detecting alerts based on data in Application Insights
        /// </summary>
        ApplicationInsights,
    }
}