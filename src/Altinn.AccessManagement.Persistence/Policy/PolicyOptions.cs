using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <summary>
/// Options for configuring storage account
/// </summary>
public class PolicyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyOptions"/> class.
    /// </summary>
    public PolicyOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyOptions"/> class.
    /// </summary>
    public PolicyOptions(PolicyAccountType account, string accountName, string container, string uri, string key, int leaseAcquireTimeoutInSec = 3)
    {
        Account = account;
        AccountName = accountName;
        Container = container;
        Uri = uri;
        Key = key;
        LeaseAcquireTimeout = TimeSpan.FromSeconds(leaseAcquireTimeoutInSec);
    }

    /// <summary>
    /// Specifies Storage Account. Mostly used as a symbol for consumer for making it simpler to targeting storage account
    /// </summary>
    public PolicyAccountType Account { get; set; }

    /// <summary>
    /// AzureRM account name
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    /// Container name
    /// </summary>
    public string Container { get; set; }

    /// <summary>
    /// Blob URI
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// SAS key for authentication
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Timeout for some specific operations. Defaults to 3 seconds
    /// </summary>
    public TimeSpan LeaseAcquireTimeout { get; set; } = TimeSpan.FromSeconds(3);
}