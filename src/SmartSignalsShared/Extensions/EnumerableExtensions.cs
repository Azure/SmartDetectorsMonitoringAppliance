//-----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for the <see cref="IEnumerable{T}"/> class
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Applies the specified action to each member of the specified collection
        /// </summary>
        /// <typeparam name="T">The type of entity in the specified collection</typeparam>
        /// <param name="collection">The collection</param>
        /// <param name="action">The action to apply</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T obj in collection)
            {
                action(obj);
            }
        }
    }
}