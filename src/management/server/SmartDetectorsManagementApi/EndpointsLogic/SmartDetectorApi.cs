//-----------------------------------------------------------------------
// <copyright file="SmartDetectorApi.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// This class is the logic for the /smartDetector endpoint.
    /// </summary>
    public class SmartDetectorApi : ISmartDetectorApi
    {
        /// <summary>
        /// The Smart Detectors repository
        /// </summary>
        private readonly ISmartDetectorRepository smartDetectorRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorApi"/> class.
        /// </summary>
        /// <param name="smartDetectorRepository">The Smart Detector repository.</param>
        public SmartDetectorApi(ISmartDetectorRepository smartDetectorRepository)
        {
            Diagnostics.EnsureArgumentNotNull(() => smartDetectorRepository);

            this.smartDetectorRepository = smartDetectorRepository;
        }

        /// <summary>
        /// List all the Smart Detectors.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Detectors.</returns>
        /// <exception cref="SmartDetectorsManagementApiException">This exception is thrown when we failed to retrieve Smart Detectors.</exception>
        public async Task<ListSmartDetectorsResponse> GetSmartDetectorsAsync(CancellationToken cancellationToken)
        {
            try
            {
                IList<SmartDetectorManifest> smartDetectorManifests = await this.smartDetectorRepository.ReadAllSmartDetectorsManifestsAsync(cancellationToken);

                // Convert Smart Detectors to the required response
                var detectors = smartDetectorManifests.Select(this.CreateSmartDetectorFromManifest).ToList();

                return new ListSmartDetectorsResponse()
                {
                    SmartDetectors = detectors
                };
            }
            catch (Exception e)
            {
                throw new SmartDetectorsManagementApiException("Failed to get Smart Detectors", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets a Smart Detector.
        /// </summary>
        /// <param name="detectorId">The detector ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Detector</returns>
        /// <exception cref="SmartDetectorsManagementApiException">This exception is thrown when we failed to retrieve the Smart Detector.</exception>
        public async Task<SmartDetector> GetSmartDetectorAsync(string detectorId, CancellationToken cancellationToken)
        {
            try
            {
                SmartDetectorManifest manifest = await this.smartDetectorRepository.ReadSmartDetectorManifestAsync(detectorId, cancellationToken);

                return this.CreateSmartDetectorFromManifest(manifest);
            }
            catch (SmartDetectorNotFoundException)
            {
                throw new SmartDetectorsManagementApiException($"Smart Detector {detectorId} was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                throw new SmartDetectorsManagementApiException($"Failed to get Smart Detector {detectorId}", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Creates a <see cref="SmartDetector"/> from a <see cref="SmartDetectorManifest"/>
        /// </summary>
        /// <param name="manifest">The smart detector manifest</param>
        /// <returns>A <see cref="SmartDetector"/> based on the <see cref="SmartDetectorManifest"/></returns>
        private SmartDetector CreateSmartDetectorFromManifest(SmartDetectorManifest manifest)
        {
            return new SmartDetector
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Description = manifest.Description,
                SupportedCadences = new List<int>(manifest.SupportedCadencesInMinutes),
                SupportedResourceTypes = manifest.SupportedResourceTypes.Select(resourceType => (ResourceType)resourceType).ToList(),
                ParameterDefinitions = new List<DetectorParameterDefinition>(manifest.ParametersDefinitions),
                ImagePaths = manifest.ImagePaths?.Select(imagePath => new Uri(imagePath)).ToList()
            };
        }
    }
}
