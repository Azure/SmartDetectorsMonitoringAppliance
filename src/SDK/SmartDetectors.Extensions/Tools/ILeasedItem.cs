﻿//-----------------------------------------------------------------------
// <copyright file="ILeasedItem.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions.Tools
{
    using System;

    /// <summary>
    /// Interface for a leased item for <see cref="ObjectPool{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    internal interface ILeasedItem<T> : IDisposable
    {
        /// <summary>
        /// Gets the leased item.
        /// </summary>
        T Item { get; }
    }
}
