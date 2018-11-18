//-----------------------------------------------------------------------
// <copyright file="DisconnectedItem.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    /// <summary>
    /// Illustrate a class that represents a row of a "{{DisconnectedItem}}" object for test puspose.
    /// This kind of object is being used by the WPF framework whenever a container is removed from the visual tree, either because the
    /// corresponding item was deleted, or the collection was refreshed, or the container was scrolled off the screen and re-virtualized.
    /// </summary>
    public class DisconnectedItem
    {
        public override string ToString()
        {
            return "{DisconnectedItem}";
        }
    }
}
