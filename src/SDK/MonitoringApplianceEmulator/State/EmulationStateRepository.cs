//-----------------------------------------------------------------------
// <copyright file="EmulationStateRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.State
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a persistent repository for storing Smart Detector related data (state) while
    /// it is being executed by the emulator.
    /// </summary>
    public class EmulationStateRepository : IStateRepository
    {
        private const int MaxSerializedStateLength = 1024 * 1024 * 1024;

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> SemaphoreRepository =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        private readonly string repositoryDir;

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStateRepository"/> class.
        /// </summary>
        /// <param name="smartDetectorId">the smart detector id</param>
        public EmulationStateRepository(string smartDetectorId)
        {
            var smartDetectorIdentifier = GenerateSmartDetectorIdentifier(smartDetectorId);
            this.repositoryDir = GenerateRepositoryDirectory(smartDetectorIdentifier);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStateRepository"/> class.
        /// This constructor is being used by unit tests in order to manipulate the repository file path.
        /// </summary>
        /// <param name="smartDetectorId">the smart detector id</param>
        /// <param name="repositoryDir">the repository dir</param>
        public EmulationStateRepository(string smartDetectorId, string repositoryDir)
            : this(smartDetectorId)
        {
            this.repositoryDir = repositoryDir;
        }

        #endregion

        #region IStateRepository Implementation

        /// <summary>
        /// Stores <paramref name="state"/> in the repository with the specified <paramref name="key"/>.
        /// If there is already a state stored with the same key, it will be replaced by <paramref name="state"/>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will store the state in the repository as a JSON-serialized string.</typeparam>
        /// <param name="key">The state's key (case insensitive).</param>
        /// <param name="state">The state to store.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key or the state are null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state serialization fails.</exception>
        /// <exception cref="StateTooBigException">This exception is thrown if serialized state exceeds allowed length.</exception>
        /// <exception cref="FailedToSaveStateException">This exception is thrown if state saving failed due to an internal error.</exception>
        public async Task StoreStateAsync<T>(string key, T state, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            key = key.ToUpperInvariant();

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            string serializedState;
            try
            {
                serializedState = JsonConvert.SerializeObject(state);
            }
            catch (Exception ex)
            {
                throw new StateSerializationException(ex);
            }

            if (serializedState.Length > MaxSerializedStateLength)
            {
                throw new StateTooBigException(serializedState.Length, MaxSerializedStateLength);
            }

            var semaphore = await GetSemaphore(key).WaitAsync(cancellationToken);
            using (semaphore)
            {
                File.WriteAllText(this.GetStateFilePath(key), serializedState);
            }
        }

        /// <summary>
        /// Deletes the state specified by <paramref name="key"/> from the repository.
        /// </summary>
        /// <param name="key">The key of the state to delete (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="FailedToDeleteStateException">This exception is thrown if state deletion failed due to an internal error.</exception>
        public async Task DeleteStateAsync(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            key = key.ToUpperInvariant();

            var semaphore = await GetSemaphore(key).WaitAsync(cancellationToken);
            using (semaphore)
            {
                var path = this.GetStateFilePath(key);
                if (File.Exists(path))
                {
                    File.Delete(path);

                    // File is only marked for deletion, wait until it actual is deleted
                    for (int i = 0; i < 10; i++)
                    {
                        if (File.Exists(path))
                        {
                            await Task.Delay(100, cancellationToken);
                            continue;
                        }

                        break;
                    }

                    if (File.Exists(path))
                    {
                        throw new IOException("Unable to delete file.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a Smart Detector's state that was saved with <paramref name="key"/>.
        /// If state does not exist, returns <code>default(<typeparamref name="T"/>)</code>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will try to JSON-deserialize the stored state to this type.</typeparam>
        /// <param name="key">The key that was used to store the state (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation, returning the requested state.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state deserialization fails.</exception>
        /// <exception cref="FailedToLoadStateException">This exception is thrown if state retrieval failed due to an internal error.</exception>
        public async Task<T> GetStateAsync<T>(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            key = key.ToUpperInvariant();
            var statePath = this.GetStateFilePath(key);

            var semaphore = await GetSemaphore(key).WaitAsync(cancellationToken);
            using (semaphore)
            {
                if (File.Exists(statePath))
                {
                    var serializedState = File.ReadAllText(statePath);
                    var state = JsonConvert.DeserializeObject<T>(serializedState);
                    return state;
                }
            }

            return default(T);
        }

        #endregion

        /// <summary>
        /// Gets a semaphore from the semaphore repository
        /// </summary>
        /// <param name="key">Key of the semaphore</param>
        /// <returns>The semaphore</returns>
        private static DisposableSemaphoreSlim GetSemaphore(string key)
        {
            var semaphore = SemaphoreRepository.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            return new DisposableSemaphoreSlim(semaphore);
        }

        /// <summary>
        /// Generates unique identifier string for the given detector id that can be used as a valid path and file name.
        /// </summary>
        /// <param name="smartDetectorId">The smart detector id</param>
        /// <returns>A unique identifier string for the detector</returns>
        private static string GenerateSmartDetectorIdentifier(string smartDetectorId)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            string validId = Regex.Replace(smartDetectorId, invalidChars, string.Empty);

            // To avoid possible duplications, add the detector's id hash code if necessary
            return validId == smartDetectorId ? $"{validId}" : $"{validId}_{smartDetectorId.ToSha256Hash()}";
        }

        /// <summary>
        /// Generates the full path (including the name) of the file that should contain all detector's state repository.
        /// </summary>
        /// <param name="smartDetectorIdentifier">Detector id for generating state directory path</param>
        /// <returns>the file path</returns>
        private static string GenerateRepositoryDirectory(string smartDetectorIdentifier)
        {
            // Get the folder for the roaming current user
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Creates a 'SmartAlertsEmulator' folder
            string emulatorPath = Path.Combine(appDataFolderPath, "SmartAlertsEmulator");

            // Creates a 'States' folder
            string statesPath = Path.Combine(emulatorPath, "States");

            // Creates a folder for the detector
            string detectorStatePath = Path.Combine(statesPath, smartDetectorIdentifier);
            Directory.CreateDirectory(detectorStatePath);

            return detectorStatePath;
        }

        /// <summary>
        /// Generates safe file name for key since key can contain invalid characters
        /// </summary>
        /// <param name="key">Key to be used for filename</param>
        /// <returns>Safe file name</returns>
        private string GetStateFilePath(string key)
        {
            return Path.Combine(this.repositoryDir, key.ToSha256Hash());
        }
    }
}
