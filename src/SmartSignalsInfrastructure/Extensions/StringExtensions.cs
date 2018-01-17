//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.Extensions
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Extension methods for string objects
    /// </summary>
    public static class StringExtensions
    {
        private static readonly HashAlgorithm HashAlgorithm = SHA256.Create();

        /// <summary>
        /// Gets the hash of the specified string
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>The 256 bit hash value, represented by a string of 64 characters</returns>
        public static string Hash(this string s)
        {
            // Get the hash bytes
            byte[] hashBytes = HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(s));

            // Convert to string
            StringBuilder hash = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hash.AppendFormat("{0:x2}", b);
            }

            return hash.ToString();
        }
    }
}