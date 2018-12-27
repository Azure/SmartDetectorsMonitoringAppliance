//-----------------------------------------------------------------------
// <copyright file="PredicatePropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// An attribute defining the predicates of an Alert.
    /// Predicate properties are used to determine if two Alerts are equivalent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PredicatePropertyAttribute : Attribute
    {
    }
}
