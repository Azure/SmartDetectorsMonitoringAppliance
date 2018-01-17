//-----------------------------------------------------------------------
// <copyright file="ChildProcessManagerTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsInfrastructureTests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TestChildProcess;

    [TestClass]
    public class ChildProcessManagerTests
    {
        private const string ChildProcessName = "TestChildProcess.exe";

        private Mock<ITracer> tracerMock;
        private IChildProcessManager childProcessManager;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();
            this.childProcessManager = new ChildProcessManager(this.tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenRunningChildProcessThenTheParentGetsTheResult()
        {
            // Run the child process and make sure we get the expected output
            TestChildProcessOutput result = await this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Happy), default(CancellationToken));
            Assert.AreEqual(TestChildProcessOutput.ExpectedIntValue, result.IntValue, "Unexpected number received from child process");
            Assert.AreEqual(TestChildProcessOutput.ExpectedStringValue, result.StringValue, "Unexpected message received from child process");
            this.EnsureProcessStopped();
        }

        [TestMethod]
        public async Task WhenRunningChildProcessAndTheResultIsNullThenTheParentGetsTheResult()
        {
            // Run the child process and make sure we get a null output
            TestChildProcessOutput result = await this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Null), default(CancellationToken));
            Assert.IsNull(result, "Expected NULL result from child process");
            this.EnsureProcessStopped();
        }

        [TestMethod]
        public void WhenRunningChildProcessAndItIsCanceledThenTheParentGetsTheCorrectException()
        {
            // Run the child process
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task<TestChildProcessOutput> t = this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Cancellation), cancellationTokenSource.Token);

            // Wait till the child process started to run
            SpinWait.SpinUntil(() => t.Status > TaskStatus.Running || this.childProcessManager.CurrentStatus != RunChildProcessStatus.Initializing);
            Assert.AreEqual(RunChildProcessStatus.WaitingForProcessToExit, this.childProcessManager.CurrentStatus, "Unexpected flow in parent process - expected to wait for child process");

            // Cancel the child process
            cancellationTokenSource.Cancel();

            try
            {
                t.Wait(TimeSpan.FromSeconds(10));
                Assert.Fail("Child process was not canceled or was taking too long");
            }
            catch (AggregateException e) when (e.InnerExceptions.Single() is ChildProcessException && e.InnerExceptions.Single().InnerException is TaskCanceledException)
            {
            }

            this.EnsureProcessStopped();
        }

        [TestMethod]
        public void WhenRunningChildProcessAndItIsStuckThenTheParentKillsItEventually()
        {
            // Run the child process
            this.childProcessManager.CancellationGraceTimeInSeconds = 1; // allow only 1 second grace time - we don't want to wait for too long
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task<TestChildProcessOutput> t = this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Stuck), cancellationTokenSource.Token);

            // Wait till the child process started to run
            SpinWait.SpinUntil(() => t.Status > TaskStatus.Running || this.childProcessManager.CurrentStatus != RunChildProcessStatus.Initializing);
            Assert.AreEqual(RunChildProcessStatus.WaitingForProcessToExit, this.childProcessManager.CurrentStatus, "Unexpected flow in parent process - expected to wait for child process");

            // Cancel the child process
            cancellationTokenSource.Cancel();

            try
            {
                t.Wait(TimeSpan.FromSeconds(10));
                Assert.Fail("Child process was not killed in an appropriate amount of time");
            }
            catch (AggregateException e) when (e.InnerExceptions.Single() is ChildProcessTerminatedByParentException)
            {
                // We should get a ChildProcessTerminatedByParentException
            }

            this.EnsureProcessStopped();
        }

        [TestMethod]
        public async Task WhenRunningChildProcessAndItThrowsAnExceptionThenTheParentGetsTheException()
        {
            // Run the child process and make sure the correct exception is thrown
            try
            {
                await this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Exception), default(CancellationToken));
                Assert.Fail("Child process did not throw an exception");
            }
            catch (ChildProcessException e) when (e.InnerException is SmartSignalRepositoryException)
            {
            }

            this.EnsureProcessStopped();
        }

        [TestMethod]
        public async Task WhenRunningChildProcessAndItCrashesThenTheExpectedExceptionIsthrown()
        {
            // Run the child process and make sure the correct exception is thrown
            try
            {
                await this.childProcessManager.RunChildProcessAsync<TestChildProcessOutput>(ChildProcessName, this.GetInput(RunMode.Crash), default(CancellationToken));
                Assert.Fail("Child process did not throw an exception");
            }
            catch (ChildProcessException e) when (e.Message == "The child process returned empty results")
            {
                // Expected exception
            }

            this.EnsureProcessStopped();
        }

        private void EnsureProcessStopped()
        {
            int runningChildProcessCount = 0;
            foreach (int childProcessId in this.childProcessManager.ChildProcessIds)
            {
                Process childProcess;
                try
                {
                    childProcess = Process.GetProcessById(childProcessId);
                }
                catch (ArgumentException)
                {
                    // The process was not found - good
                    continue;
                }

                // Verify that the process completed, and if it didn't, kill it
                if (!childProcess.HasExited)
                {
                    childProcess.Kill();
                    runningChildProcessCount++;
                }
            }

            // Make sure that the child process is not running
            Assert.AreEqual(0, runningChildProcessCount, "The child process is still running");
        }

        private TestChildProcessInput GetInput(RunMode mode)
        {
            return new TestChildProcessInput()
            {
                IntValue = TestChildProcessInput.ExpectedIntValue,
                StringValue = TestChildProcessInput.ExpectedStringValue,
                RunMode = mode
            };
        }
    }
}