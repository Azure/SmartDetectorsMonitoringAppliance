namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// An enumeration of possible components where Smart Signal detection properties can be presented.
    /// </summary>
    [Flags]
    public enum DetectionPresentationComponent
    {
        /// <summary>
        /// Indicates a property belonging to the details component.
        /// </summary>
        Details = 1,

        /// <summary>
        /// Indicates a property belonging to the summary component.
        /// </summary>
        Summary = 2,
    }
}