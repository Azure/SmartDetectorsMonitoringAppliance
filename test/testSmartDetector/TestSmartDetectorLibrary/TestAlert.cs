//-----------------------------------------------------------------------
// <copyright file="TestAlert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    public class TestAlert : Alert
    {
        public TestAlert(string title, ResourceIdentifier resourceIdentifier)
            : base(title, resourceIdentifier, DateTime.UtcNow)
        {
        }
    }
}