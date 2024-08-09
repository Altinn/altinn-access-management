using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <inheritdoc/>
public class PolicyFactoryMock(ILogger<PolicyRepositoryMock> logger) : IPolicyFactory
{
    /// <inheritdoc/>
    public IPolicyRepository Create(PolicyAccountType account, string filepath)
    {
        return new PolicyRepositoryMock(filepath, logger);
    }

    /// <inheritdoc/>
    public IPolicyRepository Create(string filepath)
    {
        return new PolicyRepositoryMock(filepath, logger);
    }
}