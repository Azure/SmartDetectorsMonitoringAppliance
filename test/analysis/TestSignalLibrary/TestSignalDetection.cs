//-----------------------------------------------------------------------
// <copyright file="TestSignalDetection.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using Microsoft.Azure.Monitoring.SmartSignals;

    public class TestSignalDetection : SmartSignalDetection
    {
        public TestSignalDetection(string title, ResourceIdentifier resourceIdentifier)
        {
            this.Title = title;
            this.ResourceIdentifier = resourceIdentifier;
        }

        public override string Title { get; }

        public override ResourceIdentifier ResourceIdentifier { get; }
    }
}