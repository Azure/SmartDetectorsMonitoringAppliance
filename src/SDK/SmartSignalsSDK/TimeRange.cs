namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// Representation of a time range with specific start and end time
    /// </summary>
    public struct TimeRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRange"/> structure with the specified start and end times.
        /// </summary>
        /// <param name="startTime">The start time of the time range.</param>
        /// <param name="endTime">The end time of the time range.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startTime"/> is greater than <paramref name="endTime"/>.
        /// </exception>
        public TimeRange(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
            {
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "The time range start time must not be greater than the end time");
            }

            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Duration = endTime - startTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRange"/> structure with the specified start time and duration.
        /// </summary>
        /// <param name="startTime">The start time of the time range.</param>
        /// <param name="duration">The duration of the time range.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="duration"/> represents a negative time span.
        /// </exception>
        public TimeRange(DateTime startTime, TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "The time range duration cannot represent a negative time span");
            }

            this.StartTime = startTime;
            this.Duration = duration;
            this.EndTime = startTime + duration;
        }

        /// <summary>
        /// Gets the start time for this range.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets the end time for this range.
        /// </summary>
        public DateTime EndTime { get; }

        /// <summary>
        /// Gets the duration of this range (basically <see cref="EndTime"/> - <see cref="StartTime"/>).
        /// </summary>
        public TimeSpan Duration { get; }

        #region Overrides of ValueType

        /// <summary>
        /// Determines whether two specified time ranges have the same value.
        /// </summary>
        /// <param name="a">The first time range to compare.</param>
        /// <param name="b">The second time range to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> represent the same time range; otherwise, false.</returns>
        public static bool operator ==(TimeRange a, TimeRange b)
        {
            return
                a.StartTime == b.StartTime &&
                a.EndTime == b.EndTime;
        }

        /// <summary>
        /// Determines whether two specified time ranges have different values.
        /// </summary>
        /// <param name="a">The first time range to compare.</param>
        /// <param name="b">The second time range to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> do not represent the same time range; otherwise, false.</returns>
        public static bool operator !=(TimeRange a, TimeRange b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns>
        /// true if <paramref name="value"/> is an instance of <see cref="TimeRange"/> and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object value)
        {
            return value is TimeRange && this == (TimeRange)value;
        }

        /// <summary>
        /// Returns the hash code for this time range.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            // Disable overflow - just in case
            unchecked
            {
                int hash = 27;
                hash = (31 * hash) + this.StartTime.GetHashCode();
                hash = (31 * hash) + this.EndTime.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this instance
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return $"[{this.StartTime:u} - {this.EndTime:u}]";
        }

        #endregion
    }
}
