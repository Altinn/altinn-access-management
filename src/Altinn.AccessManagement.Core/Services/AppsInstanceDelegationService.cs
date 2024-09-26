using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
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
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Helpers;
using System.Threading;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using System.ComponentModel.DataAnnotations;
using Altinn.Urn.Json;
using static Altinn.AccessManagement.Core.Resolvers.BaseUrn.Altinn;
using System.Linq;
using Altinn.Urn;
using System.Text.Json;

namespace Altinn.AccessManagement.Core.Services.Implementation;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public class AppsInstanceDelegationService : IAppsInstanceDelegationService
{
    private readonly ILogger<AppsInstanceDelegationService> _logger;
    private readonly IDelegationMetadataRepository _delegationRepository;
    private readonly IPartiesClient _partiesClient;
    private readonly IPolicyInformationPoint _pip;
    private readonly IPolicyAdministrationPoint _pap;
    private readonly IResourceRegistryClient _resourceRegistryClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationService"/> class.
    /// </summary>
    public AppsInstanceDelegationService(ILogger<AppsInstanceDelegationService> logger, IDelegationMetadataRepository delegationRepository, IPartiesClient partiesClient, IResourceRegistryClient resourceRegistryClient, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap)
    {
        _logger = logger;
        _delegationRepository = delegationRepository;
        _partiesClient = partiesClient;
        _pip = pip;
        _resourceRegistryClient = resourceRegistryClient;
        _pap = pap;
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
            delegationType = UuidType.Person;
            uuid = party.PartyUuid;
        }

        return (delegationType, uuid);
    }

    private bool CheckIfInstanceIsDelegable(List<Right> delegableRights, RightV2 rightToDelegate)
    {
        return delegableRights.Any(delegableRight => InstanceRightComparesEqualToDelegableRight(delegableRight, rightToDelegate));
    }

    private bool InstanceRightComparesEqualToDelegableRight(Right right, RightV2 instanceRight)
    {
        foreach (var resourcePart in right.Resource)
        {
            bool valid = instanceRight.Resource.Any(r => r.Value.PrefixSpan.ToString() == resourcePart.Id && r.Value.ValueSpan.ToString() == resourcePart.Value);

            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryGetSignificantResourcePartsFromResource(IEnumerable<UrnJsonTypeValue> input, out List<UrnJsonTypeValue> resource, string resourceTag)
    {
        resource = new List<UrnJsonTypeValue>();
        
        if (input == null || !input.Any())
        {
            return false;
        }

        bool hasOrg = false, hasApp = false, hasResource = false;
        
        foreach (UrnJsonTypeValue urnJsonTypeValue in input)
        {
            if (urnJsonTypeValue.HasValue)
            {
                switch (urnJsonTypeValue.Value.PrefixSpan.ToString())
                {
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute:
                        resource.Add(urnJsonTypeValue);
                        hasOrg = true;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute:
                        resource.Add(urnJsonTypeValue);
                        hasApp = true;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute:
                        resource.Add(urnJsonTypeValue);
                        hasResource = true;
                        break;
                }
            }
        }

        if (hasOrg && hasApp && !hasResource)
        {
            return DelegationHelper.GetResourceStringFromUrnJsonTypeEnumerable(resource) == resourceTag;
        }

        if (!hasOrg && !hasApp && hasResource)
        {
            return resource[0].Value.ValueSpan.ToString() == resourceTag;
        }

        return false;
    }

    private void AddValidationErrorsForResourceInstance(ref ValidationErrorBuilder errors, IEnumerable<RightV2> rights, string resourceid)
    {
        TryGetSignificantResourcePartsFromResource(rights.FirstOrDefault()?.Resource, out List<UrnJsonTypeValue> firstResource, resourceid);
        int counter = -1;

        foreach (RightV2 rightV2 in rights)
        {
            counter++;

            bool valid = TryGetSignificantResourcePartsFromResource(rightV2.Resource, out List<UrnJsonTypeValue> currentResource, resourceid);
            if (!valid)
            {
                errors.Add(ValidationErrors.InvalidResource, $"Rights[{counter}]/Resource");
                continue;
            }

            foreach (UrnJsonTypeValue urnJsonTypeValue in currentResource)
            {
                if (!firstResource.Contains(urnJsonTypeValue))
                {
                    errors.Add(ValidationErrors.InvalidResource, $"Rights[{counter}]/Resource");
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AppsInstanceDelegationResponse>> Delegate(AppsInstanceDelegationRequest appInstanceDelegationRequest, CancellationToken cancellationToken = default)
    {
        AppsInstanceDelegationResponse result = new AppsInstanceDelegationResponse();

        ValidationErrorBuilder errors = default;

        // Fetch from and to from partyuuid
        (UuidType Type, Guid? Uuid) from = await TranslatePartyUuidToPersonOrganizationUuid(appInstanceDelegationRequest.From);
        (UuidType Type, Guid? Uuid) to = await TranslatePartyUuidToPersonOrganizationUuid(appInstanceDelegationRequest.To);

        if (from.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "From");
        }

        if (to.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "To");
        }

        // Validate Resource and instance 1. All rights include resource 2. All rights resource is identical to central value
        AddValidationErrorsForResourceInstance(ref errors, appInstanceDelegationRequest.Rights, appInstanceDelegationRequest.ResourceId);

        // Fetch rights valid for delegation
        ServiceResource resource = (await _resourceRegistryClient.GetResourceList()).Find(r => r.Identifier == appInstanceDelegationRequest.ResourceId);
        List<Right> delegableRights = null;

        if (resource == null)
        {
            errors.Add(ValidationErrors.InvalidResource, "appInstanceDelegationRequest.Resource");
        }
        else
        {
            RightQueryForApp resourceQuery = new RightQueryForApp { OwnerApp = appInstanceDelegationRequest.PerformedBy, Resource = resource.AuthorizationReference };

            try
            {
                delegableRights = await _pip.GetDelegableRigtsForApp(resourceQuery, cancellationToken);
            }
            catch (ValidationException)
            {
                errors.Add(ValidationErrors.MissingPolicy, "appInstanceDelegationRequest.Resource");
            }

            if (delegableRights == null || delegableRights.Count == 0)
            {
                errors.Add(ValidationErrors.MissingDelegableRights, "appInstanceDelegationRequest.Resource");
            }
        }

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        result.From = appInstanceDelegationRequest.From;
        result.To = appInstanceDelegationRequest.To;
        result.Resource = appInstanceDelegationRequest.ResourceId;
        result.Instance = appInstanceDelegationRequest.InstanceId;
        result.InstanceDelegationMode = appInstanceDelegationRequest.InstanceDelegationMode;

        // Perform delegation
        DelegationHelper.TryGetPerformerFromAttributeMatches(appInstanceDelegationRequest.PerformedBy, out string performedById, out UuidType performedByType);
        InstanceRight rulesToDelegate = new InstanceRight
        {
            FromUuid = (Guid)from.Uuid,
            FromType = from.Type,
            ToUuid = (Guid)to.Uuid,
            ToType = to.Type,
            PerformedBy = performedById,
            PerformedByType = performedByType,
            ResourceId = appInstanceDelegationRequest.ResourceId,
            Instance = appInstanceDelegationRequest.InstanceId,
            InstanceDelegationMode = appInstanceDelegationRequest.InstanceDelegationMode
        };
        List<RightV2> rightsAppCantDelegate = new List<RightV2>();
        List<RightV2DelegationResult> rights = new List<RightV2DelegationResult>();
        UrnJsonTypeValue instanceId = KeyValueUrn.CreateUnchecked($"{AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute}:{appInstanceDelegationRequest.InstanceId}", AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute.Length + 1);
        
        foreach (RightV2 rightToDelegate in appInstanceDelegationRequest.Rights)
        {
            if (CheckIfInstanceIsDelegable(delegableRights, rightToDelegate))
            {
                rightToDelegate.Resource.Add(instanceId);
                rulesToDelegate.InstanceRules.Add(new InstanceRule
                {
                    Resource = rightToDelegate.Resource,
                    Action = rightToDelegate.Action
                });
            }
            else
            {
                rightsAppCantDelegate.Add(rightToDelegate);
            }
        }

        InstanceRight delegationResult = await _pap.TryWriteInstanceDelegationPolicyRules(rulesToDelegate, cancellationToken);
        rights.AddRange(DelegationHelper.GetRightDelegationResultsFromInstanceRules(delegationResult));

        if (rightsAppCantDelegate.Any())
        {
            rights.AddRange(DelegationHelper.GetRightDelegationResultsFromFailedRightV2s(rightsAppCantDelegate));
        }

        result.Rights = rights;
        
        return result;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Get()
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc/>
    public Task<Result<bool>> Revoke()
    {
        throw new NotImplementedException();
    }
}
