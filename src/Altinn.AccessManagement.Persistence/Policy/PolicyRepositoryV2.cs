using System.Diagnostics;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <inheritdoc/>
public class PolicyRepositoryV2(BlobClient client) : IPolicyRepositoryV2
{
    /// <summary>
    /// Azure Blob Storage client
    /// </summary>
    public BlobClient Client { get; } = client;

    private static async Task<T> RoundTripper<T>(Func<Task<T>> func)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
        try
        {
            return await func();
        }
        catch (RequestFailedException ex)
        {
            activity.StopWithError(ex);
            throw;
        }
        catch (Exception ex)
        {
            activity.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Response> DeletePolicyVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.WithVersion(version).DeleteAsync(cancellationToken: cancellationToken));
        return result;
    }

    /// <inheritdoc/>
    public async Task<Stream> GetPolicyAsync(CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.DownloadStreamingAsync(cancellationToken: cancellationToken));
        return result.Value.Content;
    }

    /// <inheritdoc/>
    public async Task<Stream> GetPolicyVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.WithVersion(version).DownloadStreamingAsync(cancellationToken: cancellationToken));
        return result.Value.Content;
    }

    /// <inheritdoc/>
    public async Task<bool> PolicyExistsAsync(CancellationToken cancellationToken = default)
    {
        return await RoundTripper(async () => await Client.ExistsAsync(cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async void ReleaseBlobLease(string leaseId, CancellationToken cancellationToken = default)
    {
        await RoundTripper(async () => await Client.GetBlobLeaseClient(leaseId).ReleaseAsync(cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<string> TryAcquireBlobLease(CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.GetBlobLeaseClient().AcquireAsync(TimeSpan.FromSeconds(3)));
        return result.Value.LeaseId;
    }

    /// <inheritdoc/>
    public async Task<Response<BlobContentInfo>> WritePolicyAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        return await RoundTripper(async () => await Client.UploadAsync(fileStream, true, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<Response<BlobContentInfo>> WritePolicyConditionallyAsync(Stream fileStream, string blobLeaseId, CancellationToken cancellationToken = default)
    {
        return await RoundTripper(async () => await Client.UploadAsync(
            fileStream,
            new BlobUploadOptions()
            {
                Conditions = new()
                {
                    LeaseId = blobLeaseId,
                }
            },
            cancellationToken));
    }
}

/// <summary>
/// Interface for operations on policy files.
/// </summary>
public interface IPolicyRepositoryV2
{
    /// <summary>
    /// Gets file stream for the policy file from blob storage, if it exists at the specified path.
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>File stream of the policy file</returns>
    Task<Stream> GetPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file stream for the specified version of a policy file from blob storage, if it exists at the specified path.
    /// </summary>
    /// <param name="version">The blob storage version</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>File stream of the policy file</returns>
    Task<Stream> GetPolicyVersionAsync(string version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a file stream to blobstorage to the specified path.
    /// </summary>
    /// <param name="fileStream">File stream of the policy file to be written</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Azure response BlobContentInfo</returns>
    Task<Response<BlobContentInfo>> WritePolicyAsync(Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a file stream to blobstorage to the specified path, including the conditional check that the provided blob lease id is valid.
    /// </summary>
    /// <param name="fileStream">File stream of the policy file to be written</param>
    /// <param name="blobLeaseId">The blob lease id, required to be able to write after a lock</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Azure response BlobContentInfo</returns>
    Task<Response<BlobContentInfo>> WritePolicyConditionallyAsync(Stream fileStream, string blobLeaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific version of a blob storage file if it exits on the specified path.
    /// </summary>
    /// <param name="version">The blob storage version</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<Response> DeletePolicyVersionAsync(string version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to acquire a blob lease on the base blob for the provided filepath.
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The LeaseId if a release was possible, otherwise null</returns>
    Task<string> TryAcquireBlobLease(CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a blob lease on the base blob for the provided filepath using the provided leaseId.
    /// </summary>
    /// <param name="leaseId">The lease id from to release</param>
    /// <param name="cancellationToken">cancellation token</param>
    void ReleaseBlobLease(string leaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether there exists a blob at the specified path
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Bool whether the blob exists or not</returns>
    Task<bool> PolicyExistsAsync(CancellationToken cancellationToken = default);
}