using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <inheritdoc/>
public class PolicyFactoryMock(ILogger<PolicyRepositoryMock> logger) : IPolicyFactory
{
    private ILogger<PolicyRepositoryMock> Logger { get; } = logger;

    /// <inheritdoc/>
    public IPolicyRepository Create(PolicyAccountType account, string filepath)
    {
        return new PolicyRepositoryMock(filepath, Logger);
    }

    /// <inheritdoc/>
    public IPolicyRepository Create(string filepath)
    {
        return new PolicyRepositoryMock(filepath, Logger);
    }
}