//-----------------------------------------------------------------------
// <copyright file="PageableLogArchiveTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PageableLogArchiveTests
    {
        private const string LogsFolder = @"..\..\..";
        private const string LogsFilename = @"..\..\..\logs.zip";
        private const int TestPageSize = 50;

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(LogsFilename))
            {
                File.Delete(LogsFilename);
            }
        }

        [TestMethod]
        public void WhenCreatingPageableLogArchiveThenLogArchiveIsCreatedEmpty()
        {
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            Assert.IsFalse(pageableLogArchive.LogNames.Any(), "Expected to get an empty log archive");

            AssertArchiveFile(expectedNumberOfEntries: 0);
        }

        [TestMethod]
        public async Task WhenGettingLogFromEmptyArchiveThenLogIsCreatedOnce()
        {
            // Create the log tracer and validate
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            IPageableLog log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            AssertEmptyLogTracer(log, "mylog");

            AssertArchiveFile(expectedNumberOfEntries: 1);
            AssertEmptyArchiveLogEntry("mylog");

            // Create the log tracer again, and validate that nothing has happened
            pageableLogArchive = new PageableLogArchive(LogsFolder);
            log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            AssertEmptyLogTracer(log, "mylog");

            AssertArchiveFile(expectedNumberOfEntries: 1);
            AssertEmptyArchiveLogEntry("mylog");
        }

        [TestMethod]
        public async Task WhenGettingTwoLogsFromEmptyArchiveThenLogsAreCreated()
        {
            // Create the first log tracer and validate
            var pageableLogArchive = new PageableLogArchive(LogsFolder);
            IPageableLog log = await pageableLogArchive.GetLogAsync("mylog", TestPageSize);
            AssertEmptyLogTracer(log, "mylog");

            AssertArchiveFile(expectedNumberOfEntries: 1);
            AssertEmptyArchiveLogEntry("mylog");

            // Create the second log tracer and validate
            pageableLogArchive = new PageableLogArchive(LogsFolder);
            log = await pageableLogArchive.GetLogAsync("mylog2", TestPageSize);
            AssertEmptyLogTracer(log, "mylog2");

            AssertArchiveFile(expectedNumberOfEntries: 2);
            AssertEmptyArchiveLogEntry("mylog");
            AssertEmptyArchiveLogEntry("mylog2");
        }

        private static void AssertArchiveFile(int expectedNumberOfEntries)
        {
            Assert.IsTrue(File.Exists(LogsFilename), "Archive file does not exist");
            using (ZipArchive logsArchive = ZipFile.OpenRead(LogsFilename))
            {
                Assert.AreEqual(expectedNumberOfEntries, logsArchive.Entries.Count, "Wrong number of log archive entries");
            }
        }

        private static void AssertEmptyArchiveLogEntry(string entryName)
        {
            using (ZipArchive logsArchive = ZipFile.OpenRead(LogsFilename))
            {
                Assert.IsTrue(logsArchive.Entries.Any(entry => entry.Name == entryName), $"Did not find log entry '{entryName}'");
                Assert.AreEqual(0, logsArchive.Entries.First().Length, $"Log entry '{entryName}' should be empty");
            }
        }

        private static void AssertEmptyLogTracer(IPageableLog logTracer, string logName)
        {
            Assert.AreEqual(TestPageSize, logTracer.PageSize, $"Mismatch on the log's page size for '{logName}'");
            Assert.AreEqual(0, logTracer.CurrentPageIndex, $"Mismatch on the log's current page for '{logName}'");
            Assert.AreEqual(0, logTracer.CurrentPageStart, $"Mismatch on the log's current page start for '{logName}'");
            Assert.AreEqual(0, logTracer.CurrentPageEnd, $"Mismatch on the log's current page end for '{logName}'");
            Assert.AreEqual(0, logTracer.NumberOfPages, $"Mismatch on the log's number of pages for '{logName}'");
            Assert.AreEqual(0, logTracer.NumberOfTraceLines, $"Mismatch on the log's number of trace lines for '{logName}'");
            Assert.AreEqual(logName, logTracer.Name, $"Mismatch on the log's name for '{logName}'");
            Assert.AreEqual(0, logTracer.CurrentPageTraces.Count, $"The log's current traces should have been empty for '{logName}'");
        }
    }
}
