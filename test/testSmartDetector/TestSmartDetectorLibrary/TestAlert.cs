//-----------------------------------------------------------------------
// <copyright file="TestAlert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;

    public class TestAlert : Alert
    {
        public TestAlert(string title, ResourceIdentifier resourceIdentifier)
            : base(title, resourceIdentifier)
        {
           this.ActivityLog = new TestArmRequestActivityLog(resourceIdentifier.SubscriptionId);
        }

        [TextProperty("Display name")]
        public string MyText { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [LongTextProperty("BeforeArmLongTextReference", Order = 2)]
        public string LongTextReference => "longTextReferencePathNotArm";

        [AzureResourceManagerRequestProperty]
        public TestArmRequestActivityLog ActivityLog { get; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [TextProperty("AfterArmTextReference", Order = 9)]
        public string TextReference => "textReferencePathNotArm";
    }
}
