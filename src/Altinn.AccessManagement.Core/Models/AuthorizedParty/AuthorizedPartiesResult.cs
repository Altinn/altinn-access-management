namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Wrapper model for the response from a AuthorizedParties lookup request
/// </summary>
public class AuthorizedPartiesResult
{
    /// <summary>
    /// The list of authorized parties
    /// </summary>
    public List<AuthorizedParty> AuthorizedParties { get; set; }
}