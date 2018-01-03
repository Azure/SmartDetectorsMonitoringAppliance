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
        private Dictionary<string, SmartSignalManifest> manifests;
        private Dictionary<string, Dictionary<string, byte[]>> assemblies;

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

            this.manifests = new Dictionary<string, SmartSignalManifest>()
            {
                ["1"] = new SmartSignalManifest("1", "Test signal", "Test signal description", Version.Parse("1.0"), "TestSignalLibrary", "TestSignalLibrary.TestSignal", new List<ResourceType>() { ResourceType.Subscription }),
                ["2"] = new SmartSignalManifest("2", "Test signal with dependency", "Test signal with dependency description", Version.Parse("1.0"), "TestSignalLibrary", "TestSignalLibrary.TestSignalWithDependency", new List<ResourceType>() { ResourceType.Subscription })
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
            ISmartSignalLoader loader = new SmartSignalLoader(this.tracerMock.Object);
            SmartSignalManifest manifest = new SmartSignalManifest("3", "simple", "description", Version.Parse("1.0"), signalType.Assembly.GetName().Name, signalType.FullName, new List<ResourceType>() { ResourceType.Subscription });
            SmartSignalPackage package = new SmartSignalPackage(manifest, this.assemblies["3"]);
            ISmartSignal signal = loader.LoadSignal(package);
            Assert.IsNotNull(signal, "Signal is NULL");

            var resource = ResourceIdentifier.Create(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new List<ResourceIdentifier> { resource },
                DateTime.UtcNow.AddDays(-1),
                TimeSpan.FromDays(1),
                new Mock<IAnalysisServicesFactory>().Object);
            SmartSignalResult signalResult = await signal.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, signalResult.ResultItems.Count, "Incorrect number of result items returned");
            Assert.AreEqual(expectedTitle, signalResult.ResultItems.Single().Title, "Result item title is wrong");
            Assert.AreEqual(resource, signalResult.ResultItems.Single().ResourceIdentifier, "Result item resource identifier is wrong");
        }

        private async Task TestLoadSignalFromDll(string signalId, string expectedTitle)
        {
            ISmartSignalLoader loader = new SmartSignalLoader(this.tracerMock.Object);
            SmartSignalPackage package = new SmartSignalPackage(this.manifests[signalId], this.assemblies[signalId]);
            ISmartSignal signal = loader.LoadSignal(package);
            Assert.IsNotNull(signal, "Signal is NULL");

            var resource = ResourceIdentifier.Create(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new List<ResourceIdentifier> { resource }, 
                DateTime.UtcNow.AddDays(-1), 
                TimeSpan.FromDays(1), 
                new Mock<IAnalysisServicesFactory>().Object);
            SmartSignalResult signalResult = await signal.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, signalResult.ResultItems.Count, "Incorrect number of result items returned");
            Assert.AreEqual(expectedTitle, signalResult.ResultItems.Single().Title, "Result item title is wrong");
            Assert.AreEqual(resource, signalResult.ResultItems.Single().ResourceIdentifier, "Result item resource identifier is wrong");
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

        private class TestResultItem : SmartSignalResultItem
        {
            public TestResultItem(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
            {
            }
        }

        private class TestSignal : ISmartSignal
        {
            public Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                SmartSignalResult smartSignalResult = new SmartSignalResult();
                smartSignalResult.ResultItems.Add(new TestResultItem("test test test", analysisRequest.TargetResources.Single()));
                return Task.FromResult(smartSignalResult);
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

            public Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                SmartSignalResult smartSignalResult = new SmartSignalResult();
                smartSignalResult.ResultItems.Add(new TestResultItem(this.message, analysisRequest.TargetResources.Single()));
                return Task.FromResult(smartSignalResult);
            }
        }

        private class TestSignalGeneric<T> : ISmartSignal
        {
            public Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                SmartSignalResult smartSignalResult = new SmartSignalResult();
                smartSignalResult.ResultItems.Add(new TestResultItem(typeof(T).Name, analysisRequest.TargetResources.Single()));
                return Task.FromResult(smartSignalResult);
            }
        }
    }
}