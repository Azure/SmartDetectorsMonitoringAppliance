//-----------------------------------------------------------------------
// <copyright file="SmartSignalLoaderTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// The smart signal loader tests rely on signals that are defined in TestSignalLibrary and TestSignalDependentLibrary.
    /// These DLLs are not directly referenced, but only copied to the output directory - so we can test how the loader dynamically
    /// loads them and runs their signals.
    /// </summary>
    [TestClass]
    public class SmartSignalLoaderTests
    {
        private Dictionary<string, DllInfo> dllInfos;
        private Mock<ITracer> tracerMock;
        private Dictionary<string, SmartSignalMetadata> metadatas;
        private Dictionary<string, Dictionary<string, byte[]>> assemblies;
        private Mock<ISmartSignalsRepository> smartSignalsRepositoryMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();

            // Handle DLLs
            this.dllInfos = new Dictionary<string, DllInfo>();
            this.InitializeDll("TestSignalLibrary");
            this.InitializeDll("TestSignalDependentLibrary");
            this.InitializeDll("Newtonsoft.Json");

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            this.metadatas = new Dictionary<string, SmartSignalMetadata>()
            {
                ["1"] = new SmartSignalMetadata("1", "Test signal", "Test signal description", "1.0", "TestSignalLibrary", "TestSignalLibrary.TestSignal", new List<ResourceType>() { ResourceType.Subscription }),
                ["2"] = new SmartSignalMetadata("2", "Test signal with dependency", "Test signal with dependency description", "1.0", "TestSignalLibrary", "TestSignalLibrary.TestSignalWithDependency", new List<ResourceType>() { ResourceType.Subscription })
            };

            this.assemblies = new Dictionary<string, Dictionary<string, byte[]>>
            {
                ["1"] = new Dictionary<string, byte[]>()
                {
                    ["TestSignalLibrary"] = this.dllInfos["TestSignalLibrary"].Bytes
                },
                ["2"] = new Dictionary<string, byte[]>()
                {
                    ["TestSignalLibrary"] = this.dllInfos["TestSignalLibrary"].Bytes,
                    ["TestSignalDependentLibrary"] = this.dllInfos["TestSignalDependentLibrary"].Bytes,
                    ["Newtonsoft.Json"] = this.dllInfos["Newtonsoft.Json"].Bytes
                },
                ["3"] = new Dictionary<string, byte[]>()
                {
                    [currentAssembly.GetName().Name] = File.ReadAllBytes(currentAssembly.Location)
                }
            };

            this.smartSignalsRepositoryMock = new Mock<ISmartSignalsRepository>();
            this.smartSignalsRepositoryMock
                .Setup(x => x.ReadSignalMetadataAsync(It.IsAny<string>()))
                .ReturnsAsync((string signalId) => this.metadatas[signalId]);
            this.smartSignalsRepositoryMock
                .Setup(x => x.ReadSignalAssembliesAsync(It.IsAny<string>()))
                .ReturnsAsync((string signalId) => this.assemblies[signalId]);
        }

        [TestMethod]
        public async Task WhenLoadingSignalFromDllThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSignalFromDll("1", "test title");
        }

        [TestMethod]
        public async Task WhenLoadingSignalFromMultipleDllsThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSignalFromDll("2", "test title - with dependency - [1,2,3]");
        }

        [TestMethod]
        public async Task WhenLoadingSignalTheInstanceIsCreatedSuccessfully()
        {
            await this.TestLoadSignalSimple(typeof(TestSignal));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartSignalLoadException))]
        public async Task WhenLoadingSignalWithWrongTypeThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalNoInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartSignalLoadException))]
        public async Task WhenLoadingSignalWithoutDefaultConstructorThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalNoDefaultConstructor));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartSignalLoadException))]
        public async Task WhenLoadingSignalWithGenericDefinitionThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalGeneric<>));
        }

        [TestMethod]
        public async Task WhenLoadingSignalWithConcreteGenericDefinitionThenItWorks()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalGeneric<string>), typeof(string).Name);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Rename DLLs back
            foreach (var info in this.dllInfos.Values)
            {
                File.Delete(info.OriginalPath);
                File.Move(info.NewPath, info.OriginalPath);
            }
        }

        private async Task TestLoadSignalSimple(Type signalType, string expectedTitle = "test test test")
        {
            ISmartSignalLoader loader = new SmartSignalLoader(this.smartSignalsRepositoryMock.Object, this.tracerMock.Object);
            SmartSignalMetadata metadata = new SmartSignalMetadata("3", "simple", "description", "1.0", signalType.Assembly.GetName().Name, signalType.FullName, new List<ResourceType>() { ResourceType.Subscription });
            ISmartSignal signal = await loader.LoadSignalAsync(metadata);
            Assert.IsNotNull(signal, "Signal is NULL");

            List<SmartSignalDetection> detections = await signal.AnalyzeResourcesAsync(null, new TimeRange(), null, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, detections.Count, "Incorrect number of detections returned");
            Assert.AreEqual(expectedTitle, detections.Single().Title, "Detection title is wrong");
        }

        private async Task TestLoadSignalFromDll(string signalId, string expectedTitle)
        {
            ISmartSignalLoader loader = new SmartSignalLoader(this.smartSignalsRepositoryMock.Object, this.tracerMock.Object);
            ISmartSignal signal = await loader.LoadSignalAsync(this.metadatas[signalId]);
            Assert.IsNotNull(signal, "Signal is NULL");

            List<SmartSignalDetection> detections = await signal.AnalyzeResourcesAsync(null, new TimeRange(), null, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, detections.Count, "Incorrect number of detections returned");
            Assert.AreEqual(expectedTitle, detections.Single().Title, "Detection title is wrong");
        }

        private void InitializeDll(string name)
        {
            // Determine DLL path
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (folder == null)
            {
                throw new NullReferenceException();
            }

            string path = Path.Combine(folder, name + ".dll");

            // Read DLL bytes
            byte[] bytes = File.ReadAllBytes(path);

            // Rename the DLL
            string newPath = Path.ChangeExtension(path, ".x.dll");
            File.Delete(newPath);
            File.Move(path, newPath);

            // Save information
            this.dllInfos[name] = new DllInfo()
            {
                OriginalPath = path,
                NewPath = newPath,
                Bytes = bytes
            };
        }

        private class DllInfo
        {
            public string OriginalPath { get; set; }

            public string NewPath { get; set; }

            public byte[] Bytes { get; set; }
        }

        private class TestDetection : SmartSignalDetection
        {
            public TestDetection(string s = "test test test")
            {
                this.Title = s;
            }

            public override string Title { get; }
        }

        private class TestSignal : ISmartSignal
        {
            public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
            {
                return Task.FromResult(new List<SmartSignalDetection>() { new TestDetection() });
            }
        }

        private class TestSignalNoInterface
        {
        }

        private class TestSignalNoDefaultConstructor : ISmartSignal
        {
            private readonly string message;

            public TestSignalNoDefaultConstructor(string message)
            {
                this.message = message;
            }

            public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
            {
                return Task.FromResult(new List<SmartSignalDetection>() { new TestDetection(this.message) });
            }
        }

        private class TestSignalGeneric<T> : ISmartSignal
        {
            public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
            {
                return Task.FromResult(new List<SmartSignalDetection>() { new TestDetection(typeof(T).Name) });
            }
        }
    }
}