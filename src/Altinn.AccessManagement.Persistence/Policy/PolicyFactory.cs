using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <inheritdoc/>
public class PolicyFactory(IAzureClientFactory<BlobServiceClient> factory, IOptionsFactory<PolicyOptions> options) : IPolicyFactory
{
    /// <summary>
    /// Factory for creating clients
    /// </summary>
    /// <value>asd</value>
    public IAzureClientFactory<BlobServiceClient> Factory { get; } = factory;

    /// <summary>
    /// Factory for getting configuration for client factory
    /// </summary>
    public IOptionsFactory<PolicyOptions> Options { get; } = options;

    /// <inheritdoc/>
    public IPolicyRepositoryV2 Create(AccountType account, string blob)
    {
        var options = Options.Create(account.ToString());
        var client = Factory
            .CreateClient(account.ToString())
            .CreateBlobContainer(options.Container).Value
            .GetBlobClient(blob);

        return new PolicyRepositoryV2(client, options);
    }
}

/// <summary>
/// Create clients for interacting with files 
/// </summary>
public interface IPolicyFactory
{
    /// <summary>
    /// Creates a client for interacting with storage
    /// </summary>
    /// <param name="account">which storage account to write blob</param>
    /// <param name="filepath">path of the file</param>
    /// <returns></returns>
    IPolicyRepositoryV2 Create(AccountType account, string filepath);
}