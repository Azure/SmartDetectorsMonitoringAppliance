namespace $safeprojectname$
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;

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
        [PredicateProperty]
        [TextProperty("Resource name", Order = 1)]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the number of records
        /// </summary>
        [TextProperty("Number of records", Order = 2)]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the query that displays the number of records over time
        /// </summary>
        [ChartProperty("Number of records over time for {ResourceName}", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis, Order = 3)]
        public List<ChartPoint> CountChart { get; set; }
    }
}