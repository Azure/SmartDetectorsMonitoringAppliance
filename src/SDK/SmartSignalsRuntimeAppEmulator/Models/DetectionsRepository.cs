namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Detections repository model. Holds all detections created in the current run.
    /// </summary>
    public class DetectionsRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetectionsRepository"/> class.
        /// </summary>
        public DetectionsRepository()
        {
            this.Detections = new ObservableCollection<SmartSignalDetection>();
        }

        /// <summary>
        /// Gets the collection of detections in the repository.
        /// </summary>
        public ObservableCollection<SmartSignalDetection> Detections { get; }
    }
}
