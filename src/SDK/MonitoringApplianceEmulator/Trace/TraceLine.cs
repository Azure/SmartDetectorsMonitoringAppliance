//-----------------------------------------------------------------------
// <copyright file="TraceLine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// A class for holding a specific trace line
    /// </summary>
    public class TraceLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLine"/> class.
        /// </summary>
        /// <param name="level">The level of the trace line (info, verbose etc.).</param>
        /// <param name="timeStamp">The time stamp in which the trace line was emitted.</param>
        /// <param name="message">The trace line's message.</param>
        public TraceLine(TraceLevel level, DateTime timeStamp, string message)
        {
            this.Level = level;
            this.TimeStamp = timeStamp;
            this.Message = message;
        }

        /// <summary>
        /// Gets the level of the trace line (info, verbose etc.).
        /// </summary>
        public TraceLevel Level { get; }

        /// <summary>
        /// Gets the time stamp in which the trace line was emitted.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Gets the trace line's message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Parses <paramref name="traceLine"/> to a <see cref="TraceLine"/> object. The trace line is assumed to be
        /// of the same format we use in the <see cref="Compose"/> method: {level}|{time stamp}|{message}
        /// </summary>
        /// <param name="traceLine">The trace line to parse.</param>
        /// <returns>The parsed trace line.</returns>
        public static TraceLine Parse(string traceLine)
        {
            string[] lineParts = traceLine.Split('|');
            if (lineParts.Length < 3)
            {
                throw new InvalidOperationException($"Unable to parse trace line '{traceLine}'");
            }

            if (!Enum.TryParse(lineParts[0], out TraceLevel traceLevel))
            {
                throw new InvalidOperationException($"Invalid trace level for trace line '{traceLine}'");
            }

            if (!DateTime.TryParseExact(lineParts[1], "yyyy-MM-dd HH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime traceTimestamp))
            {
                throw new InvalidOperationException($"Invalid trace time stamp for trace line '{traceLine}'");
            }

            return new TraceLine(traceLevel, traceTimestamp, string.Join("|", lineParts.Skip(2)));
        }

        /// <summary>
        /// Composes a string representation of the current trace line, with the format of: {level}|{time stamp}|{message}
        /// </summary>
        /// <returns>The string representation of the trace line.</returns>
        public string Compose()
        {
            return $"{this.Level}|{this.TimeStamp:yyyy-MM-dd HH:mm:ss.fffZ}|{this.Message}";
        }
    }
}
