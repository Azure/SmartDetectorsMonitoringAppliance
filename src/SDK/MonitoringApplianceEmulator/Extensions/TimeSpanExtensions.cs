//-----------------------------------------------------------------------
// <copyright file="TimeSpanExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for <see cref="TimeSpan"/> objects.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Gets a readable string representation of a time span.
        /// </summary>
        /// <param name="timeSpan">The time span</param>
        /// <returns>Readable string representation</returns>
        public static string ToReadableString(this TimeSpan timeSpan)
        {
            string days = timeSpan.Duration().Days > 0 ?
                $"{timeSpan.Days} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " :
                string.Empty;

            string hours = timeSpan.Duration().Hours > 0 ?
                $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " :
                string.Empty;

            string minutes = timeSpan.Duration().Minutes > 0 ?
                $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")}, " :
                string.Empty;

            string seconds = timeSpan.Duration().Seconds > 0 ?
                $"{timeSpan.Seconds} second{(timeSpan.Seconds == 1 ? string.Empty : "s")}" :
                string.Empty;

            string readableString = $"{days}{hours}{minutes}{seconds}";

            if (readableString.EndsWith(", ", StringComparison.InvariantCulture))
            {
                readableString = readableString.Substring(0, readableString.Length - 2);
            }

            if (string.IsNullOrEmpty(readableString))
            {
                readableString = "0 seconds";
            }

            return readableString;
        }
    }
}
