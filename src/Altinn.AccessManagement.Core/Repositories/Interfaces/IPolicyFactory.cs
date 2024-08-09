using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Repositories.Interfaces;

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
    IPolicyRepository Create(PolicyAccountType account, string filepath);

    /// <summary>
    /// Creates a client for interacting with storage. assuming storage accoutn based on filename.
    /// </summary>
    /// <param name="filepath">path of the file</param>
    /// <returns></returns>
    IPolicyRepository Create(string filepath);
}