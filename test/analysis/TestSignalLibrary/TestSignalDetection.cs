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
        public TestSignalDetection(string title)
        {
            this.Title = title;
        }

        public override string Title { get; }
    }
}