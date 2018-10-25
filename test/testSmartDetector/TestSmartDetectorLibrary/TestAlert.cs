//-----------------------------------------------------------------------
// <copyright file="TestAlert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using Microsoft.Azure.Monitoring.SmartDetectors;

    public class TestAlert : Alert
    {
        public TestAlert(string title, ResourceIdentifier resourceIdentifier, AlertState state)
            : base(title, resourceIdentifier, state)
        {
        }
    }
}