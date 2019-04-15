//-----------------------------------------------------------------------
// <copyright file="SmartDetectorLoaderTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    /// <summary>
    /// The Smart Detector loader tests rely on detectors that are defined in TestSmartDetectorLibrary and TestSmartDetectorDependentLibrary.
    /// These DLLs are not directly referenced, but only copied to the output directory - so we can test how the loader dynamically
    /// loads them and runs their Smart Detectors.
    /// </summary>
    [TestClass]
    public class SmartDetectorLoaderTests
    {
        private Dictionary<string, DllInfo> dllInfos;
        private Mock<ITracer> tracerMock;
        private Dictionary<string, SmartDetectorManifest> manifests;
        private Dictionary<string, Dictionary<string, byte[]>> assemblies;
        private string tempFolder;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.dllInfos = new Dictionary<string, DllInfo>();

            this.tracerMock = new Mock<ITracer>();

            // Cleanup data from previous tests (cleaning in TestCleanup will not work since the DLLs are in use)
            string tempFolderParent = Path.Combine(Path.GetTempPath(), "SmartDetectorLoaderTests");
            if (Directory.Exists(tempFolderParent))
            {
                foreach (string childFolder in Directory.GetDirectories(tempFolderParent))
                {
                    try
                    {
                        Directory.Delete(childFolder, true);
                    }
                    catch
                    {
                        // Ignored
                    }
                }
            }

            // Create temp folder
            this.tempFolder = Path.Combine(tempFolderParent, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(this.tempFolder);

            // For package load test, skip remaining initializations
            string name = this.TestContext.TestName;
            if (name == nameof(this.WhenLoadingSmartDetectorFromPackageThenItWorks))
            {
                return;
            }

            // Handle DLLs
            this.InitializeDll("TestSmartDetectorLibrary");
            this.InitializeDll("TestSmartDetectorDependentLibrary");
            this.InitializeDll("Newtonsoft.Json");

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            this.manifests = new Dictionary<string, SmartDetectorManifest>()
            {
                ["1"] = new SmartDetectorManifest("1", "Test Smart Detector", "Test Smart Detector description", Version.Parse("1.0"), "TestSmartDetectorLibrary.dll", "TestSmartDetectorLibrary.TestSmartDetector", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 }, null, null),
                ["2"] = new SmartDetectorManifest("2", "Test Smart Detector with dependency", "Test Smart Detector with dependency description", Version.Parse("1.0"), "TestSmartDetectorLibrary.dll", "TestSmartDetectorLibrary.TestSmartDetectorWithDependency", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 }, null, null)
            };

            this.assemblies = new Dictionary<string, Dictionary<string, byte[]>>
            {
                ["1"] = new Dictionary<string, byte[]>()
                {
                    ["TestSmartDetectorLibrary.dll"] = this.dllInfos["TestSmartDetectorLibrary"].Bytes
                },
                ["2"] = new Dictionary<string, byte[]>()
                {
                    ["TestSmartDetectorLibrary.dll"] = this.dllInfos["TestSmartDetectorLibrary"].Bytes,
                    ["TestSmartDetectorDependentLibrary.dll"] = this.dllInfos["TestSmartDetectorDependentLibrary"].Bytes,
                    ["Newtonsoft.Json.dll"] = this.dllInfos["Newtonsoft.Json"].Bytes
                },
                ["3"] = new Dictionary<string, byte[]>()
                {
                    [currentAssembly.GetName().Name + ".dll"] = File.ReadAllBytes(currentAssembly.Location)
                }
            };
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorFromDllThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSmartDetectorFromDll("1", "test title");
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorFromMultipleDllsThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSmartDetectorFromDll("2", "test title - with dependency - [1,2,3]");
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorTheInstanceIsCreatedSuccessfully()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetector));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithWrongTypeThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorNoInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithoutDefaultConstructorThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorNoDefaultConstructor));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithGenericDefinitionThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorGeneric<>));
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorWithConcreteGenericDefinitionThenItWorks()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorGeneric<string>), typeof(string).Name);
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorFromPackageThenItWorks()
        {
            // Create the detector package from the detector's folder (instead of from the current folder),
            // and check that it can be loaded correctly, with its dependencies.
            DirectoryInfo currentFolder = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            string flavor = Path.Combine(currentFolder.Parent.Name, currentFolder.Name);
            while (currentFolder.Name.ToUpperInvariant() != "TEST")
            {
                currentFolder = currentFolder.Parent;
            }

            string packageFolder = Path.Combine(currentFolder.FullName, @"testSmartDetector\TestSmartDetectorLibrary\bin", flavor);
            SmartDetectorPackage package = SmartDetectorPackage.CreateFromFolder(packageFolder);
            await this.TestLoadSmartDetectorSimple(package, "test title - with dependency - [1,2,3]");
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

        private async Task TestLoadSmartDetectorSimple(Type smartDetectorType, string expectedTitle = "test test test")
        {
            SmartDetectorManifest manifest = new SmartDetectorManifest("3", "simple", "description", Version.Parse("1.0"), smartDetectorType.Assembly.GetName().Name, smartDetectorType.FullName, new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 }, null, null);
            Dictionary<string, byte[]> packageContent = this.assemblies["3"];
            packageContent["manifest.json"] = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(manifest));
            SmartDetectorPackage package = new SmartDetectorPackage(packageContent);
            await this.TestLoadSmartDetectorSimple(package, expectedTitle);
        }

        private async Task TestLoadSmartDetectorSimple(SmartDetectorPackage package, string expectedTitle)
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tempFolder, this.tracerMock.Object);
            ISmartDetector detector = loader.LoadSmartDetector(package);
            Assert.IsNotNull(detector, "Smart Detector is NULL");

            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new AnalysisRequestParameters(
                    DateTime.UtcNow,
                    new List<ResourceIdentifier> { resource },
                    TimeSpan.FromDays(1),
                    null,
                    null),
                new Mock<IAnalysisServicesFactory>().Object,
                new Mock<IStateRepository>().Object);
            List<Alert> alerts = await detector.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, alerts.Count, "Incorrect number of alerts returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Alert title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Alert resource identifier is wrong");
        }

        private async Task TestLoadSmartDetectorFromDll(string smartDetectorId, string expectedTitle)
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tempFolder, this.tracerMock.Object);
            Dictionary<string, byte[]> packageContent = this.assemblies[smartDetectorId];
            packageContent["manifest.json"] = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this.manifests[smartDetectorId]));
            SmartDetectorPackage package = new SmartDetectorPackage(packageContent);
            ISmartDetector detector = loader.LoadSmartDetector(package);
            Assert.IsNotNull(detector, "Smart Detector is NULL");

            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new AnalysisRequestParameters(
                    DateTime.UtcNow,
                    new List<ResourceIdentifier> { resource },
                    TimeSpan.FromDays(1),
                    null,
                    null),
                new Mock<IAnalysisServicesFactory>().Object,
                new Mock<IStateRepository>().Object);
            List<Alert> alerts = await detector.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, alerts.Count, "Incorrect number of alerts returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Alert title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Alert resource identifier is wrong");
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

        private class TestAlert : Alert
        {
            public TestAlert(string title, ResourceIdentifier resourceIdentifier)
                : base(title, resourceIdentifier)
            {
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by reflection")]
        private class TestSmartDetector : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert("test test test", analysisRequest.RequestParameters.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by reflection")]
        private class TestSmartDetectorNoInterface
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by reflection")]
        private class TestSmartDetectorNoDefaultConstructor : ISmartDetector
        {
            private readonly string message;

            public TestSmartDetectorNoDefaultConstructor(string message)
            {
                this.message = message;
            }

            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert(this.message, analysisRequest.RequestParameters.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by reflection")]
        private class TestSmartDetectorGeneric<T> : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert(typeof(T).Name, analysisRequest.RequestParameters.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }
    }
}