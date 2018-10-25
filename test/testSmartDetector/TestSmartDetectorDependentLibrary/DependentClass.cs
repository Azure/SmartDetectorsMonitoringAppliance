//-----------------------------------------------------------------------
// <copyright file="DependentClass.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorDependentLibrary
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class DependentClass
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        public string GetString()
        {
            return "with dependency";
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        public string ObjectToString(object serializeMe)
        {
            return JsonConvert.SerializeObject(serializeMe);
        }
    }
}
