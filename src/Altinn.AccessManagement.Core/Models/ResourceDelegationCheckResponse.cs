using Altinn.AccessManagement.Core.Models.Register;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Response model for the result of a delegation status check, for which rights a user is able to delegate between two parties.
/// </summary>
public class ResourceDelegationCheckResponse
{
    /// <summary>
    /// Gets or sets the urn identifying the party the rights can be delegated from
    /// </summary>
    public required PartyUrn From { get; set; }

    /// <summary>
    /// Gets or sets a list of right delegation status models
    /// </summary>
    public List<ResourceRightDelegationCheckResult> ResourceRightDelegationCheckResults { get; set; }
}
