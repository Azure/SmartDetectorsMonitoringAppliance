namespace $safeprojectname$
{
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// A sample implementation of an <see cref="Alert"/>. 
    /// This sample implementation provides an example of an alert with basic properties and a chart.
    /// </summary>
    public class $alertName$ : Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="$alertName$"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        /// <param name="count">The number of records found in the alert query</param>
        public $alertName$(string title, ResourceIdentifier resourceIdentifier, int count) : base(title, resourceIdentifier)
        {
            this.ResourceName = resourceIdentifier.ResourceName;
            this.Count = count;
        }

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        [AlertPredicateProperty]
        [AlertPresentationProperty(AlertPresentationSection.Property, "Resource name", Order = 1, InfoBalloon = "The resource name")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the number of records
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.Property, "Number of records", Order = 2, InfoBalloon = "The number of records")]
        public int Count { get; set; }

        /// <summary>
        /// Gets the query that displays the number of records over time
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.Chart, "Number of records over time for {ResourceName}", Order = 3, InfoBalloon = "The number of records over time")]
        public string CountChart => $@"$query$";
    }
}