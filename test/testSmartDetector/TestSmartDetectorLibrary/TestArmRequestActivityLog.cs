//-----------------------------------------------------------------------
// <copyright file="TestArmRequestActivityLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;

   public class TestArmRequestActivityLog : AzureResourceManagerRequest
    {
        public TestArmRequestActivityLog(string subscriptionId)
            : base(new Uri($"subscriptions/{subscriptionId}/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp ge '2019-07-01T22:35:00Z' and eventTimestamp le '2019-07-01T22:40:00Z'", UriKind.Relative))
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [TextProperty("TextReferenceDisplayName", Order = 1)]
        public PropertyReference TextReference => new PropertyReference("resourceGroupName");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [TextProperty("TextReferenceTheSecond", Order = 5)]
        public PropertyReference TextReference2 => new PropertyReference("eventName.value");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [LongTextProperty("LongTextReferenceDisplayName", Order = 2)]
        public PropertyReference LongTextReference => new PropertyReference("authorization.scope");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [KeyValueProperty("KeyValueReferenceDisplayName", Order = 3)]
        public PropertyReference KeyValueReference => new PropertyReference("eventDataId");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", Order = 4, ShowHeaders = true)]
        public TablePropertyReference<ReferenceTableDataActivityLog> MultiColumnTableReference => new TablePropertyReference<ReferenceTableDataActivityLog>("$");
    }
}
