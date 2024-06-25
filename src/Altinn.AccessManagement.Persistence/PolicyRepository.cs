using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Repository for handling policy files
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PolicyRepository : IPolicyRepository
    {
        private readonly AzureStorageConfiguration _storageConfig;
        private readonly BlobContainerClient _metadataContainerClient;
        private readonly BlobContainerClient _delegationsContainerClient;
        private readonly BlobContainerClient _resourceRegisterContainerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRepository"/> class
        /// </summary>
        /// <param name="storageConfig">The storage configuration for Azure Blob Storage.</param>
        public PolicyRepository(IOptions<AzureStorageConfiguration> storageConfig)
        {
            _storageConfig = storageConfig.Value;

            StorageSharedKeyCredential metadataCredentials = new StorageSharedKeyCredential(_storageConfig.MetadataAccountName, _storageConfig.MetadataAccountKey);
            BlobServiceClient metadataServiceClient = new BlobServiceClient(new Uri(_storageConfig.MetadataBlobEndpoint), metadataCredentials);
            _metadataContainerClient = metadataServiceClient.GetBlobContainerClient(_storageConfig.MetadataContainer);

            StorageSharedKeyCredential delegationsCredentials = new StorageSharedKeyCredential(_storageConfig.DelegationsAccountName, _storageConfig.DelegationsAccountKey);
            BlobServiceClient delegationsServiceClient = new BlobServiceClient(new Uri(_storageConfig.DelegationsBlobEndpoint), delegationsCredentials);
            _delegationsContainerClient = delegationsServiceClient.GetBlobContainerClient(_storageConfig.DelegationsContainer);

            StorageSharedKeyCredential resourceRegisterCredentials = new StorageSharedKeyCredential(_storageConfig.ResourceRegistryAccountName, _storageConfig.ResourceRegistryAccountKey);
            BlobServiceClient resourceRegisterServiceClient = new BlobServiceClient(new Uri(_storageConfig.ResourceRegistryBlobEndpoint), resourceRegisterCredentials);
            _resourceRegisterContainerClient = resourceRegisterServiceClient.GetBlobContainerClient(_storageConfig.ResourceRegistryContainer);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetPolicyAsync(string filepath)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath);

            return await GetBlobStreamInternal(blobClient);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetPolicyVersionAsync(string filepath, string version)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath).WithVersion(version);

            return await GetBlobStreamInternal(blobClient);
        }

        /// <inheritdoc/>
        public async Task<Response<BlobContentInfo>> WritePolicyAsync(string filepath, Stream fileStream)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath);

            return await WriteBlobStreamInternal(blobClient, fileStream);
        }

        /// <inheritdoc/>
        public async Task<Response<BlobContentInfo>> WritePolicyConditionallyAsync(string filepath, Stream fileStream, string blobLeaseId)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath);

            BlobUploadOptions blobUploadOptions = new BlobUploadOptions()
            {
                Conditions = new BlobRequestConditions()
                {
                    LeaseId = blobLeaseId
                }
            };

            return await WriteBlobStreamInternal(blobClient, fileStream, blobUploadOptions);
        }

        /// <inheritdoc/>
        public async Task<string> TryAcquireBlobLease(string filepath)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath);
            BlobLeaseClient blobLeaseClient = blobClient.GetBlobLeaseClient();

            try
            {
                BlobLease blobLease = await blobLeaseClient.AcquireAsync(TimeSpan.FromSeconds(_storageConfig.BlobLeaseTimeout));
                return blobLease.LeaseId;
            }
            catch (RequestFailedException ex)
            {
                activity?.StopWithError(ex, $"Failed to acquire blob lease for policy file at {filepath}. RequestFailedException");
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex, $"Failed to acquire blob lease for policy file at {filepath}. Unexpected error");
            }

            return null;
        }

        /// <inheritdoc/>
        public async void ReleaseBlobLease(string filepath, string leaseId)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            BlobClient blobClient = CreateBlobClient(filepath);
            BlobLeaseClient blobLeaseClient = blobClient.GetBlobLeaseClient(leaseId);
            await blobLeaseClient.ReleaseAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> PolicyExistsAsync(string filepath)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                BlobClient blobClient = CreateBlobClient(filepath);
                return await blobClient.ExistsAsync();
            }
            catch (RequestFailedException ex)
            {
                activity?.StopWithError(ex, $"Failed to check if blob exists for policy file at {filepath}. RequestFailedException");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<Response> DeletePolicyVersionAsync(string filepath, string version)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                BlobClient blockBlob = CreateBlobClient(filepath);

                return await blockBlob.WithVersion(version).DeleteAsync();
            }
            catch (RequestFailedException ex)
            {
                var errorMsg = ex.Status == (int)HttpStatusCode.Forbidden && ex.ErrorCode == "OperationNotAllowedOnRootBlob" ?
                $"Failed to delete version {version} of policy file at {filepath}. Not allowed to delete current version." :
                $"Failed to delete version {version} of policy file at {filepath}. RequestFailedException";
                activity?.StopWithError(ex, errorMsg);
                throw;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex, $"Failed to delete version {version} of policy file at {filepath}. Unexpected error");
                throw;
            }
        }

        private BlobClient CreateBlobClient(string blobName)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            if (blobName.Contains("delegationpolicy.xml"))
            {
                activity?.AddEvent(new ActivityEvent("_delegationsContainerClient.GetBlobClient"));
                return _delegationsContainerClient.GetBlobClient(blobName);
            }

            if (blobName.Contains("resourcepolicy.xml"))
            {
                activity?.AddEvent(new ActivityEvent("_resourceRegisterContainerClient.GetBlobClient"));
                return _resourceRegisterContainerClient.GetBlobClient(blobName);
            }

            activity?.AddEvent(new ActivityEvent("_metadataContainerClient.GetBlobClient"));
            return _metadataContainerClient.GetBlobClient(blobName);
        }

        private async Task<Stream> GetBlobStreamInternal(BlobClient blobClient)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                Stream memoryStream = new MemoryStream();

                if (await blobClient.ExistsAsync())
                {
                    await blobClient.DownloadToAsync(memoryStream);
                    memoryStream.Position = 0;

                    return memoryStream;
                }

                return memoryStream;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex, $"Failed to read policy file at {blobClient.Name}");
                throw;
            }
        }

        private async Task<Response<BlobContentInfo>> WriteBlobStreamInternal(BlobClient blobClient, Stream fileStream, BlobUploadOptions blobUploadOptions = null)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                if (blobUploadOptions != null)
                {
                    return await blobClient.UploadAsync(fileStream, blobUploadOptions);
                }

                return await blobClient.UploadAsync(fileStream, true);
            }
            catch (RequestFailedException ex)
            {
                activity?.StopWithError(ex, $"Failed to save policy file {blobClient.Name}. {(HttpStatusCode)ex.Status}");
                throw;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex, $"Failed to save policy file {blobClient.Name}. Unexpected exception");
                throw;
            }
        }
    }
}
