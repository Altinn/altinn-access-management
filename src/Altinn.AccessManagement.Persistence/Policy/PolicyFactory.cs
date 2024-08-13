using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <inheritdoc/>
public class PolicyFactory : IPolicyFactory
{
    /// <summary>
    /// Creates a new factory that handles blobs
    /// </summary>
    /// <param name="factory">Azure client factory for creating blob clients</param>
    /// <param name="options">options for configuring azure blob clients</param>
    public PolicyFactory(IAzureClientFactory<BlobServiceClient> factory, IOptionsFactory<PolicyOptions> options)
    {
        Factory = factory;
        Options = options;
    }

    private IAzureClientFactory<BlobServiceClient> Factory { get; }

    private IOptionsFactory<PolicyOptions> Options { get; }

    /// <inheritdoc/>
    public IPolicyRepository Create(PolicyAccountType account, string filepath)
    {
        var options = Options.Create(account.ToString());
        return new PolicyRepository(Factory.CreateClient(account.ToString()).GetBlobContainerClient(options.Container).GetBlobClient(filepath), options);
    }

    /// <inheritdoc/>
    public IPolicyRepository Create(string filepath) => filepath switch
    {
        var blob when blob.EndsWith("delegationpolicy.xml") => Create(PolicyAccountType.Delegations, filepath),
        var blob when blob.EndsWith("resourcepolicy.xml") => Create(PolicyAccountType.ResourceRegister, filepath),
        _ => Create(PolicyAccountType.Metadata, filepath),
    };
}