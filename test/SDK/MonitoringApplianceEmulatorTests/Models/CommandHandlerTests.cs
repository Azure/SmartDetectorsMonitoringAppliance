//-----------------------------------------------------------------------
// <copyright file="CommandHandlerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommandHandlerTests
    {
        [TestMethod]
        public void WhenExecutingCommandThenItWasExecuted()
        {
            bool commandActionWasExecuted = false;

            var commandHandler = new CommandHandler(parameter =>
            {
                commandActionWasExecuted = (bool)parameter;
            });

            commandHandler.Execute(parameter: true);

            Assert.IsTrue(commandActionWasExecuted);
        }
    }
}
