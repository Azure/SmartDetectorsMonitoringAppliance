namespace $safeprojectname$
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;

    /// <summary>
    /// A sample implementation of an <see cref="Alert"/>, without any additional data - only a title.
    /// </summary>
    public class $alertName$ : Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="$alertName$"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        public $alertName$(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
        }
    }
}
