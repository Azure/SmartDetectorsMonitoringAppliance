namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// An attribute defining the predicates of a Smart Signal detection.
    /// Predicate properties are used to determine if two detections are equivalent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DetectionPredicateAttribute : Attribute
    {
    }
}
