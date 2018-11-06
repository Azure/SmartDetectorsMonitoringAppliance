//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Tools;

    /// <summary>
    /// Extension methods for string objects
    /// </summary>
    public static class StringExtensions
    {
        private static readonly ObjectPool<HashAlgorithm> HashAlgoPool = new ObjectPool<HashAlgorithm>(SHA256.Create);
        private static readonly Dictionary<string, Script<string>> InterpolatedStringScripts = new Dictionary<string, Script<string>>();

        /// <summary>
        /// Gets the hash of the specified string
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>The 256 bit hash value, represented by a string of 64 characters</returns>
        public static string ToSha256Hash(this string s)
        {
            // Get the hash bytes
            byte[] hashBytes;
            using (var hashAlgo = HashAlgoPool.LeaseItem())
            {
                hashBytes = hashAlgo.Item.ComputeHash(Encoding.UTF8.GetBytes(s));
            }

            // Convert to string
            StringBuilder hash = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hash.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hash.ToString();
        }

        /// <summary>
        /// Assumes the string is an interpolated string definition, including property names
        /// from the specified object, and returns the evaluated string value.
        /// Examples:
        ///    "User {Name} with role {Role} created at {CreationDate:D}".AsInterpolatedString(user)
        ///      => "User john@contoso.com with role Administrator created at Monday, June 15, 2009
        ///    "Found {NumAffectedMachines} affected {(NumAffectedMachines == 1 ? \"machine\" : \"machines\")}"
        ///      => "Found 1 affected machine", "Found 3 affected machines"
        /// </summary>
        /// <param name="interpolatedStringDefinition">The interpolated string definition.</param>
        /// <param name="source">The object, whose properties can appear in the interpolated string definition. The object type definition must be public.</param>
        /// <returns>The evaluated result string</returns>
        internal static string EvaluateInterpolatedString(this string interpolatedStringDefinition, object source)
        {
            // If this is not an interpolated string - just return it as-is
            if (!interpolatedStringDefinition.Contains("{"))
            {
                return interpolatedStringDefinition;
            }

            // Code that interprets the string as an interpolated string
            string code = "$\"" + interpolatedStringDefinition + "\"";

            // Get the script from cache (improves performance since the script is already compiled)
            string key = source.GetType().FullName + "#" + code;
            if (!InterpolatedStringScripts.TryGetValue(key, out Script<string> script))
            {
                script = InterpolatedStringScripts[key] = CSharpScript.Create<string>(code, globalsType: source.GetType());
            }

            // Run the script and return its result
            ScriptState<string> state = script.RunAsync(globals: source).Result;
            return state.ReturnValue;
        }
    }
}