//-----------------------------------------------------------------------
// <copyright file="ExtendedDateTime.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// This class provides tools for detector authors to support both real and emulated date time
    /// in their logic
    /// </summary>
    public static class ExtendedDateTime
    {
        private static DateTime? emulatedUtcNow = null;

        /// <summary>
        /// Gets the current time in the UTC time zone. The returned value will be the
        /// same as <see cref="DateTime.UtcNow"/> when actually running the detector, and
        /// the simulated time when running in iterative mode in the Smart Detector emulator.
        /// </summary>
        public static DateTime UtcNow => emulatedUtcNow ?? DateTime.UtcNow;

        /// <summary>
        /// Sets the emulated current UTC time value. Subsequent calls to <see cref="UtcNow"/> will return
        /// the value from <paramref name="utcNow"/>.
        /// </summary>
        /// <param name="utcNow">The emulated UTC time to set.</param>
        public static void SetEmulatedUtcNow(DateTime utcNow)
        {
            emulatedUtcNow = utcNow;
        }

        /// <summary>
        /// Resets the emulated current UTC time value to return the actual time. Subsequent calls
        /// to <see cref="UtcNow"/> will return <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public static void ResetEmulatedUtcNow()
        {
            emulatedUtcNow = null;
        }
    }
}
