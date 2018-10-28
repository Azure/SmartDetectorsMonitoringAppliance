//-----------------------------------------------------------------------
// <copyright file="PageableLogFileTracerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PageableLogFileTracerTests
    {
        private const string LogsFolder = @"..\..\..";
        private const string LogsFilename = @"..\..\..\logs.zip";
        private const int TestPageSize = 10;

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(LogsFilename))
            {
                File.Delete(LogsFilename);
            }
        }

        [TestMethod]
        public async Task WhenSendingTracesThenTracesAreSaved()
        {
            // Create the log tracer, trace a bit, and validate
            using (var pageableLogArchive = new PageableLogArchive(LogsFolder))
            {
                using (IPageableLogTracer logTracer = await pageableLogArchive.GetLogAsync("mylog", TestPageSize))
                {
                    logTracer.TraceVerbose("0");
                    logTracer.TraceInformation("1");
                    logTracer.TraceWarning("2");
                    logTracer.TraceError("3");

                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(0, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(1, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(4, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(4, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");
                    AssertTraceLine(logTracer, 0, TraceLevel.Verbose, "0");
                    AssertTraceLine(logTracer, 1, TraceLevel.Info, "1");
                    AssertTraceLine(logTracer, 2, TraceLevel.Warning, "2");
                    AssertTraceLine(logTracer, 3, TraceLevel.Error, "3");
                }
            }

            // Now reopen the tracer and validate again
            using (var pageableLogArchive = new PageableLogArchive(LogsFolder))
            {
                using (IPageableLogTracer logTracer = await pageableLogArchive.GetLogAsync("mylog", TestPageSize))
                {
                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(0, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(1, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(4, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(4, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");
                    AssertTraceLine(logTracer, 0, TraceLevel.Verbose, "0");
                    AssertTraceLine(logTracer, 1, TraceLevel.Info, "1");
                    AssertTraceLine(logTracer, 2, TraceLevel.Warning, "2");
                    AssertTraceLine(logTracer, 3, TraceLevel.Error, "3");
                }
            }
        }

        [TestMethod]
        public async Task WhenSendingManyTracesThenPagesAreHandledCorrectly()
        {
            // Create the log tracer, trace a lot, and validate
            using (var pageableLogArchive = new PageableLogArchive(LogsFolder))
            {
                using (IPageableLogTracer logTracer = await pageableLogArchive.GetLogAsync("mylog", TestPageSize))
                {
                    for (int i = 0; i < TestPageSize * 10; i++)
                    {
                        logTracer.TraceInformation($"{i}");
                    }

                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(0, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(10, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(TestPageSize * 10, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(TestPageSize, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");

                    await logTracer.SetCurrentPageIndexAsync(4);

                    for (int i = 0; i < 10; i++)
                    {
                        await logTracer.SetCurrentPageIndexAsync(i);
                        AssertPageTraces(logTracer, i);
                    }
                }
            }

            // Now reopen the tracer, validate, trace some more and validate again
            using (var pageableLogArchive = new PageableLogArchive(LogsFolder))
            {
                using (IPageableLogTracer logTracer = await pageableLogArchive.GetLogAsync("mylog", TestPageSize))
                {
                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(0, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(10, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(TestPageSize * 10, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(TestPageSize, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");
                    for (int i = 0; i < 10; i++)
                    {
                        await logTracer.SetCurrentPageIndexAsync(i);
                        AssertPageTraces(logTracer, i);
                    }

                    for (int i = TestPageSize * 10; i < TestPageSize * 11; i++)
                    {
                        logTracer.TraceInformation($"{i}");
                    }

                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(9, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(11, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(TestPageSize * 11, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(TestPageSize, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");

                    for (int i = 0; i < 11; i++)
                    {
                        await logTracer.SetCurrentPageIndexAsync(i);
                        AssertPageTraces(logTracer, i);
                    }

                    // Now move forward and backward
                    await logTracer.PrevPageAsync();
                    await logTracer.PrevPageAsync();
                    AssertPageTraces(logTracer, 8);
                    await logTracer.NextPageAsync();
                    AssertPageTraces(logTracer, 9);
                }
            }
        }

        [TestMethod]
        public async Task WhenUpdatingPageSizeThenPagesAreHandledCorrectly()
        {
            // Create the log tracer, trace a lot, validate, update page size, trace and validate more
            using (var pageableLogArchive = new PageableLogArchive(LogsFolder))
            {
                using (IPageableLogTracer logTracer = await pageableLogArchive.GetLogAsync("mylog", TestPageSize))
                {
                    for (int i = 0; i < TestPageSize * 10; i++)
                    {
                        logTracer.TraceInformation($"{i}");
                    }

                    Assert.AreEqual(TestPageSize, logTracer.PageSize, "Mismatch on the tracer's page size");
                    Assert.AreEqual(0, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    Assert.AreEqual(10, logTracer.NumberOfPages, "Mismatch on the tracer's number of pages");
                    Assert.AreEqual(TestPageSize * 10, logTracer.NumberOfTraceLines, "Mismatch on the tracer's number of trace lines");
                    Assert.AreEqual("mylog", logTracer.SessionId, "Mismatch on the tracer's session ID");
                    Assert.AreEqual(TestPageSize, logTracer.CurrentPageTraces.Count, "Mismatch on the tracer's current page traces count");

                    for (int i = 0; i < 10; i++)
                    {
                        await logTracer.SetCurrentPageIndexAsync(i);
                        AssertPageTraces(logTracer, i);
                    }

                    await logTracer.SetCurrentPageIndexAsync(3);
                    await logTracer.SetPageSizeAsync(7);

                    Assert.AreEqual(4, logTracer.CurrentPageIndex, "Mismatch on the tracer's current page");
                    for (int i = 0; i < 7; i++)
                    {
                        AssertTraceLine(logTracer, i, TraceLevel.Info, $"{(4 * 7) + i}");
                    }
                }
            }
        }

        private static void AssertTraceLine(IPageableLogTracer logTracer, int traceIndex, TraceLevel expectedLevel, string message)
        {
            Assert.AreEqual(expectedLevel, logTracer.CurrentPageTraces[traceIndex].Level, $"Mismatch on trace #{traceIndex}'s level");
            Assert.AreEqual(message, logTracer.CurrentPageTraces[traceIndex].Message, $"Mismatch on trace #{traceIndex}'s message");
        }

        private static void AssertPageTraces(IPageableLogTracer logTracer, int pageIndex)
        {
            for (int i = 0; i < TestPageSize; i++)
            {
                AssertTraceLine(logTracer, i, TraceLevel.Info, $"{(pageIndex * TestPageSize) + i}");
            }
        }
    }
}
