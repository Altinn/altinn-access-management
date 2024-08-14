using System.Diagnostics;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <inheritdoc/>
public class PolicyRepository(BlobClient client, PolicyOptions options) : IPolicyRepository
{
    /// <summary>
    /// Azure Blob Storage client
    /// </summary>
    public BlobClient Client { get; } = client;

    /// <summary>
    /// Policy Options
    /// </summary>
    public PolicyOptions Options { get; } = options;

    /// <inheritdoc/>
    public async Task<Response> DeletePolicyVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.WithVersion(version).DeleteAsync(cancellationToken: cancellationToken));
        return result;
    }

    /// <inheritdoc/>
    public async Task<Stream> GetPolicyAsync(CancellationToken cancellationToken = default) =>
        await RoundTripper(async () => await DownloadStreamAsync(Client, cancellationToken: cancellationToken));

    /// <inheritdoc/>
    public async Task<Stream> GetPolicyVersionAsync(string version, CancellationToken cancellationToken = default) =>
         await RoundTripper(async () => await DownloadStreamAsync(Client.WithVersion(version), cancellationToken: cancellationToken));

    /// <inheritdoc/>
    public async Task<bool> PolicyExistsAsync(CancellationToken cancellationToken = default) =>
        await RoundTripper(async () => await Client.ExistsAsync(cancellationToken: cancellationToken));

    /// <inheritdoc/>
    public async void ReleaseBlobLease(string leaseId, CancellationToken cancellationToken = default) =>
        await RoundTripper(async () => await Client.GetBlobLeaseClient(leaseId).ReleaseAsync(cancellationToken: cancellationToken));

    /// <inheritdoc/>
    public async Task<string> TryAcquireBlobLease(CancellationToken cancellationToken = default)
    {
        var result = await RoundTripper(async () => await Client.GetBlobLeaseClient().AcquireAsync(Options.LeaseAcquireTimeout, cancellationToken: cancellationToken));
        return result.Value.LeaseId;
    }

    /// <inheritdoc/>
    public async Task<Response<BlobContentInfo>> WritePolicyAsync(Stream fileStream, CancellationToken cancellationToken = default) =>
        await RoundTripper(async () => await Client.UploadAsync(fileStream ?? new MemoryStream(), true, cancellationToken));

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

    private static async Task<Stream> DownloadStreamAsync(BlobClient client, CancellationToken cancellationToken = default)
    {
        Stream result = new MemoryStream();

        if (await client.ExistsAsync(cancellationToken))
        {
            await client.DownloadToAsync(result, cancellationToken);
            result.Position = 0;
            return result;
        }

        return result;
    }
}