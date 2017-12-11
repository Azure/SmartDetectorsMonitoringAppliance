namespace Microsoft.Azure.Monitoring.SmartSignals
{
    /// <summary>
    /// A class representing a detection made by a Smart Signal.
    /// A smart signal execution might return several detections if several issues were detected on the analyzed resources. Each detection
    /// instance contains both the detection's data and representation properties.
    /// </summary>
    public abstract class SmartSignalDetection
    {
        /// <summary>
        /// Gets the title of this detection.
        /// </summary>
        public abstract string Title { get; }
    }
}
