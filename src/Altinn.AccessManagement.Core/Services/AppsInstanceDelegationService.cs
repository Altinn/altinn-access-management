using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Platform.Register.Models;
using System;
using System.Net;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;

namespace Altinn.Platform.Authorization.Services.Implementation;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public class AppsInstanceDelegationService : IAppsInstanceDelegationService
{
    private readonly ILogger<AppsInstanceDelegationService> _logger;
    private readonly IDelegationMetadataRepository _delegationRepository;
    private readonly IPartiesClient _partiesClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationService"/> class.
    /// </summary>
    public AppsInstanceDelegationService(ILogger<AppsInstanceDelegationService> logger, IDelegationMetadataRepository delegationRepository, IPartiesClient partiesClient)
    {
        _logger = logger;
        _delegationRepository = delegationRepository;
        _partiesClient = partiesClient;
    }

    private async Task<(UuidType DelegationType, Guid? Uuid)> TranslatePartyUuidToPersonOrganizationUuid(PartyUrn partyId)
    {
        UuidType delegationType = UuidType.NotSpecified;
        Guid? uuid = null;
        Party party = null;

        if (partyId.IsOrganizationIdentifier(out OrganizationNumber orgNumber))
        {
            PartyLookup lookup = new PartyLookup
            {
                OrgNo = orgNumber.ToString()
            };

            party = await _partiesClient.LookupPartyBySSNOrOrgNo(lookup);
        }
        else if (partyId.IsPartyUuid(out Guid partyUuid))
        {
            party = (await _partiesClient.GetPartiesAsync(partyUuid.SingleToList())).FirstOrDefault();
        }

        if (party?.Organization != null)
        {
            delegationType = UuidType.Organization;
            uuid = party.PartyUuid;
        }
        else if (party?.Person != null)
        {
            delegationType = UuidType.Organization;
            uuid = party.PartyUuid;
        }

        return (delegationType, uuid);
    }
    
    /// <inheritdoc/>
    public async Task<Result<bool>> Delegate(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        ValidationErrorBuilder errors = default;

        // TODO: Authorize app

        // TODO: Verify rights valid for delegation

        // TODO: Fetch from and to from partyuuid
        (UuidType Type, Guid? Uuid) from = await TranslatePartyUuidToPersonOrganizationUuid(appInstanceDelegationRequest.From);
        (UuidType Type, Guid? Uuid) to = await TranslatePartyUuidToPersonOrganizationUuid(appInstanceDelegationRequest.To);

        if (from.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "appInstanceDelegationRequest.From");
        }

        if (to.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "appInstanceDelegationRequest.To");
        }

        // TODO: Fetch policy path from input
        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        // TODO: Fetch latest change
        InstanceDelegationChangeRequest instanceDelegationChangeRequest = new InstanceDelegationChangeRequest()
        {
            InstanceDelegationType = appInstanceDelegationRequest.InstanceDelegationType,
            Instance = appInstanceDelegationRequest.Instance,
            Resource = appInstanceDelegationRequest.Resource,
            
            FromUuid = (Guid)from.Uuid,
            FromUuidType = from.Type,
            ToUuid = Guid.Parse(appInstanceDelegationRequest.To.ValueSpan),
            ToUuidType = to.Type
        };

        InstanceDelegationChange lastChange = await _delegationRepository.GetLastInstanceDelegationChange(instanceDelegationChangeRequest);

        // TODO: Fetch policy file

        // TODO: Update policy file

        // TODO: Get blob storage lease

        // TODO: Write policy file

        // TODO: Update db and use new version from latest update
        InstanceDelegationChange instanceDelegationChange = new InstanceDelegationChange
        {
            DelegationChangeType = DelegationChangeType.Grant,
            InstanceDelegationType = appInstanceDelegationRequest.InstanceDelegationType,
            Instance = appInstanceDelegationRequest.Instance,
            Resource = appInstanceDelegationRequest.Resource,
            
            // TODO: Fetch real data here
            BlobStoragePolicyPath = "Test",
            BlobStorageVersionId = "Test",

            FromUuid = (Guid)from.Uuid,
            FromUuidType = from.Type,
            ToUuid = Guid.Parse(appInstanceDelegationRequest.To.ValueSpan),
            ToUuidType = to.Type,
            
            // TODO: Fetch real data here
            PerformedBy = "Test",
            PerformedByType = UuidType.Resource
        };

        // TODO: Add error handling db writing
        await _delegationRepository.InsertInstanceDelegation(instanceDelegationChange);
        return true;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Get(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Revoke(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        throw new NotImplementedException();
    }
}
