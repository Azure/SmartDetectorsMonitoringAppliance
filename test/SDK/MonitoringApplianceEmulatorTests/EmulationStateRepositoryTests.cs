//-----------------------------------------------------------------------
// <copyright file="EmulationStateRepositoryTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class EmulationStateRepositoryTests
    {
        private const string TestDetectorId = "test_id";

        private const string StatesDirectory = @"..\..\..\test_state";

        [TestInitialize]
        public void TestInit()
        {
            Directory.CreateDirectory(StatesDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(StatesDirectory))
            {
                Directory.Delete(StatesDirectory, true);
            }
        }

        [TestMethod]
        public void WhenCreatingNewStateRepositoryThenItLoadsPreviousStateFromFileSuccesfully()
        {
            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            IStateRepository stateRepository = new EmulationStateRepository(TestDetectorId, StatesDirectory);
            stateRepository.StoreStateAsync<TestState>("key", originalState, CancellationToken.None);

            IStateRepository newStateRepository = new EmulationStateRepository(TestDetectorId, StatesDirectory);
            TestState retrievedState = newStateRepository.GetStateAsync<TestState>("key", CancellationToken.None).Result;

            // Validate content was read from file when the new state repository was created
            Assert.AreEqual(originalState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, retrievedState.Field2);
            Assert.AreEqual(originalState.Field3, retrievedState.Field3);
            Assert.AreEqual(originalState.Field4, retrievedState.Field4);
        }

        [TestMethod]
        public void WhenRunningMultipleStateActionsInParallelThenNoExceptionIsThrown()
        {
            Random random = new Random();
            List<string> keys = Enumerable.Range(1, 100)
                .Select(num => $"{random.Next(10)}")
                .ToList();

            Parallel.ForEach(keys, key => SimulateDetectorRun(key).Wait());
        }

        [TestMethod]
        public async Task WhenExecutingBasicStateActionsThenFlowCompletesSuccesfully()
        {
            EmulationStateRepository stateRepository = new EmulationStateRepository(TestDetectorId, StatesDirectory);

            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await stateRepository.StoreStateAsync("key", originalState, CancellationToken.None);

            var retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // Validate
            Assert.AreEqual(originalState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, retrievedState.Field2);
            Assert.AreEqual(originalState.Field3, retrievedState.Field3);
            Assert.AreEqual(originalState.Field4, retrievedState.Field4);

            // Update existing state
            var updatedState = new TestState
            {
                Field1 = null,
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await stateRepository.StoreStateAsync("key", updatedState, CancellationToken.None);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // Validate
            Assert.AreEqual(updatedState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(updatedState.Field2, retrievedState.Field2);
            Assert.AreEqual(updatedState.Field3, retrievedState.Field3);
            Assert.AreEqual(updatedState.Field4, retrievedState.Field4);

            await stateRepository.StoreStateAsync("key2", originalState, CancellationToken.None);
            await stateRepository.DeleteStateAsync("key", CancellationToken.None);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            Assert.IsNull(retrievedState);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key2", CancellationToken.None);

            Assert.IsNotNull(retrievedState);
        }

        [TestMethod]
        public async Task WhenWritingStateToFileThenItIsWrittenSuccessfully()
        {
            // Create dummy state and write it to a file
            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            EmulationStateRepository stateRepository = new EmulationStateRepository(TestDetectorId, StatesDirectory);

            await stateRepository.StoreStateAsync<TestState>("key", originalState, CancellationToken.None);

            // Create new state repository
            EmulationStateRepository anotherStateRepository = new EmulationStateRepository(TestDetectorId, StatesDirectory);
            TestState actualtState = stateRepository.GetStateAsync<TestState>("key", CancellationToken.None).Result;

            // Validate content was read from file when the state repository was created
            Assert.AreEqual(originalState.Field1, actualtState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, actualtState.Field2);
            Assert.AreEqual(originalState.Field3, actualtState.Field3);
            Assert.AreEqual(originalState.Field4, actualtState.Field4);
        }

        private static async Task SimulateDetectorRun(string key)
        {
            var stateRepo = new EmulationStateRepository(TestDetectorId, StatesDirectory);

            var state = new List<int> { 5, 10 };

            await stateRepo.StoreStateAsync(key, state, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
            await stateRepo.StoreStateAsync(key, state, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
            await stateRepo.DeleteStateAsync(key, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
        }

        private class TestState
        {
            public string Field1 { get; set; }

            public List<DateTime> Field2 { get; set; }

            public bool Field3 { get; set; }

            public string Field4 { get; set; }
        }
    }
}
