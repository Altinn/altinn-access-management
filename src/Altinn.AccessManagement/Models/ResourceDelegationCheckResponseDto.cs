using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Response model for the result of a delegation status check, for which rights a user is able to delegate between two parties.
/// </summary>
public class ResourceDelegationCheckResponseDto
{
    /// <summary>
    /// Gets or sets the urn identifying the party the rights can be delegated from
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required UrnJsonTypeValue<PartyUrn> From { get; set; }

    /// <summary>
    /// Gets or sets a list of right delegation status models
    /// </summary>
    public IEnumerable<ResourceRightDelegationCheckResultDto> ResourceRightDelegationCheckResults { get; set; }
}
