using Altinn.AccessManagement.Core.Models.AccessList;

namespace Altinn.AccessManagement.Core.Clients.Interfaces;

/// <summary>
/// Access list authorization interface.
/// </summary>
public interface IAccessListsAuthorizationClient
{
    /// <summary>
    /// Authorization of a given subject for resource access through access lists.
    /// </summary>
    /// <param name="request">The <see cref="AccessListAuthorizationRequest"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AccessListAuthorizationResponse> AuthorizePartyForAccessList(AccessListAuthorizationRequest request, CancellationToken cancellationToken = default);
}