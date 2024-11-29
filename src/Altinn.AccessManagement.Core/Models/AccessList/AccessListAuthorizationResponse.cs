using System;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Models.AccessList;

/// <summary>
/// Contains attribute match info about the reportee party and resource that's to be authorized
/// </summary>
public class AccessListAuthorizationResponse
{
    /// <summary>
    /// Creates a new <see cref="AccessListAuthorizationResponse"/> from an <see cref="AccessListAuthorizationRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The mapped <see cref="AccessListAuthorizationResponse"/>.</returns>
    public static AccessListAuthorizationResponse From(AccessListAuthorizationRequest request)
    {
        request = request ?? throw new ArgumentNullException(nameof(request));

        return new AccessListAuthorizationResponse
        {
            Subject = request.Subject,
            Resource = request.Resource,
            Action = request.Action,
            Result = AccessListAuthorizationResult.NotDetermined
        };
    }

    /// <summary>
    /// Gets or sets the attributes identifying the party to be authorized
    /// </summary>
    public UrnJsonTypeValue<PartyUrn> Subject { get; set; }

    /// <summary>
    /// Gets or sets the attributes identifying the resource to authorize the party for
    /// </summary>
    public UrnJsonTypeValue<ResourceIdUrn> Resource { get; set; }

    /// <summary>
    /// Gets or sets an optional action value to authorize
    /// </summary>
    public UrnJsonTypeValue<ActionUrn> Action { get; set; }

    /// <summary>
    /// Gets or sets the result of the access list authorization
    /// </summary>
    public AccessListAuthorizationResult Result { get; set; }
}