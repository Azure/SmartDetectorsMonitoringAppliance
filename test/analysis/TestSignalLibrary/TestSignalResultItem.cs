//-----------------------------------------------------------------------
// <copyright file="TestSignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using Microsoft.Azure.Monitoring.SmartSignals;

    public class TestSignalResultItem : SmartSignalResultItem
    {
        public TestSignalResultItem(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
        }
    }
}