using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Persistence.Policy;

/// <summary>
/// Options for configuring storage account
/// </summary>
public class PolicyOptions(PolicyAccountType account, string accountName, string container, string uri, string key, int leaseAcquireTimeoutInSec = 3)
{
    /// <summary>
    /// Specifies Storage Account. Mostly used as a symbol for consumer for making it simpler to targeting storage account
    /// </summary>
    [Required]
    public PolicyAccountType Account { get; set; } = account;

    /// <summary>
    /// AzureRM account name
    /// </summary>
    [Required]
    public string AccountName { get; set; } = accountName;

    /// <summary>
    /// Container name
    /// </summary>
    [Required]
    public string Container { get; set; } = container;

    /// <summary>
    /// Blob URI
    /// </summary>
    [Required]
    public string Uri { get; set; } = uri;

    /// <summary>
    /// SAS key for authentication
    /// </summary>
    [Required]
    public string Key { get; set; } = key;

    /// <summary>
    /// Timeout for some specific operations. Defaults to 3 seconds
    /// </summary>
    public TimeSpan LeaseAcquireTimeout { get; set; } = TimeSpan.FromSeconds(leaseAcquireTimeoutInSec);
}