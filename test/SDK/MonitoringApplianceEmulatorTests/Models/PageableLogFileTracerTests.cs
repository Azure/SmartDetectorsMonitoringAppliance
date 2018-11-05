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
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            var log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            using (ILogArchiveTracer tracer = log.CreateTracer())
            {
                Assert.AreEqual("mylog", tracer.SessionId, "Mismatch on the tracer's session ID");
                tracer.TraceVerbose("0");
                tracer.TraceInformation("1");
                tracer.TraceWarning("2");
                tracer.TraceError("3");
            }

            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(0, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(0, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(3, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(1, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(4, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(4, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");
            AssertTraceLine(log, 0, TraceLevel.Verbose, "0");
            AssertTraceLine(log, 1, TraceLevel.Info, "1");
            AssertTraceLine(log, 2, TraceLevel.Warning, "2");
            AssertTraceLine(log, 3, TraceLevel.Error, "3");

            // Now reopen the tracer and validate again
            pageableLogArchive = new PageableLogArchive(LogsFolder);
            log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(0, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(0, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(3, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(1, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(4, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(4, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");
            AssertTraceLine(log, 0, TraceLevel.Verbose, "0");
            AssertTraceLine(log, 1, TraceLevel.Info, "1");
            AssertTraceLine(log, 2, TraceLevel.Warning, "2");
            AssertTraceLine(log, 3, TraceLevel.Error, "3");
        }

        [TestMethod]
        public async Task WhenSendingManyTracesThenPagesAreHandledCorrectly()
        {
            // Create the log tracer, trace a lot, and validate
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            var log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            using (ILogArchiveTracer tracer = log.CreateTracer())
            {
                for (int i = 0; i < TestPageSize * 10; i++)
                {
                    tracer.TraceInformation($"{i}");
                }
            }

            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(0, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(0, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(9, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(10, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(TestPageSize * 10, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(TestPageSize, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");

            log.CurrentPageIndex = 4;

            for (int i = 0; i < 10; i++)
            {
                log.CurrentPageIndex = i;
                AssertPageTraces(log, i);
            }

            // Now reopen the tracer, validate, trace some more and validate again
            pageableLogArchive = new PageableLogArchive(LogsFolder);
            log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(0, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(0, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(9, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(10, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(TestPageSize * 10, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(TestPageSize, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");
            for (int i = 0; i < 10; i++)
            {
                log.CurrentPageIndex = i;
                AssertPageTraces(log, i);
            }

            using (ILogArchiveTracer tracer = log.CreateTracer())
            {
                for (int i = TestPageSize * 10; i < TestPageSize * 11; i++)
                {
                    tracer.TraceInformation($"{i}");
                }
            }

            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(9, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(90, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(99, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(11, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(TestPageSize * 11, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(TestPageSize, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");

            for (int i = 0; i < 11; i++)
            {
                log.CurrentPageIndex = i;
                AssertPageTraces(log, i);
            }
        }

        [TestMethod]
        public async Task WhenUpdatingPageSizeThenPagesAreHandledCorrectly()
        {
            // Create the log tracer, trace a lot, validate, update page size, trace and validate more
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            var log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            using (ILogArchiveTracer tracer = log.CreateTracer())
            {
                for (int i = 0; i < TestPageSize * 10; i++)
                {
                    tracer.TraceInformation($"{i}");
                }
            }

            Assert.AreEqual(TestPageSize, log.PageSize, "Mismatch on the log's page size");
            Assert.AreEqual(0, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(0, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(9, log.CurrentPageEnd, "Mismatch on the log's current page end");
            Assert.AreEqual(10, log.NumberOfPages, "Mismatch on the log's number of pages");
            Assert.AreEqual(TestPageSize * 10, log.NumberOfTraceLines, "Mismatch on the log's number of trace lines");
            Assert.AreEqual("mylog", log.Name, "Mismatch on the log's name");
            Assert.AreEqual(TestPageSize, log.CurrentPageTraces.Count, "Mismatch on the log's current page traces count");

            for (int i = 0; i < 10; i++)
            {
                log.CurrentPageIndex = i;
                AssertPageTraces(log, i);
            }

            log.CurrentPageIndex = 3;
            log.PageSize = 7;

            Assert.AreEqual(4, log.CurrentPageIndex, "Mismatch on the log's current page");
            Assert.AreEqual(28, log.CurrentPageStart, "Mismatch on the log's current page start");
            Assert.AreEqual(34, log.CurrentPageEnd, "Mismatch on the log's current page end");
            for (int i = 0; i < 7; i++)
            {
                AssertTraceLine(log, i, TraceLevel.Info, $"{(4 * 7) + i}");
            }
        }

        private static void AssertTraceLine(IPageableLog logTracer, int traceIndex, TraceLevel expectedLevel, string message)
        {
            Assert.AreEqual(expectedLevel, logTracer.CurrentPageTraces[traceIndex].Level, $"Mismatch on trace #{traceIndex}'s level");
            Assert.AreEqual(message, logTracer.CurrentPageTraces[traceIndex].Message, $"Mismatch on trace #{traceIndex}'s message");
        }

        private static void AssertPageTraces(IPageableLog logTracer, int pageIndex)
        {
            for (int i = 0; i < TestPageSize; i++)
            {
                AssertTraceLine(logTracer, i, TraceLevel.Info, $"{(pageIndex * TestPageSize) + i}");
            }
        }
    }
}
