using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
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
    public IPolicyRepository Create(PolicyAccountType account, string filepath)
    {
        var config = Options.Create(account.ToString());
        var client = Factory
            .CreateClient(account.ToString())
            .CreateBlobContainer(config.Container).Value
            .GetBlobClient(filepath);

        return new PolicyRepository(client, config);
    }

    /// <inheritdoc/>
    public IPolicyRepository Create(string filepath) => filepath switch
    {
        var blob when blob.EndsWith("delegationpolicy.xml") => Create(PolicyAccountType.Delegations, filepath),
        var blob when blob.EndsWith("resourcepolicy.xml") => Create(PolicyAccountType.ResourceRegister, filepath),
        _ => Create(PolicyAccountType.Metadata, filepath),
    };
}