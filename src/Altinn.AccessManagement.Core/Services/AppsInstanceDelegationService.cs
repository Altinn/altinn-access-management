using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.Authorization.ProblemDetails;
using Altinn.Platform.Register.Models;
using Altinn.Urn;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Services.Implementation;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public class AppsInstanceDelegationService : IAppsInstanceDelegationService
{
    private readonly IPartiesClient _partiesClient;
    private readonly IPolicyInformationPoint _pip;
    private readonly IPolicyAdministrationPoint _pap;
    private readonly IResourceRegistryClient _resourceRegistryClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationService"/> class.
    /// </summary>
    public AppsInstanceDelegationService(IPartiesClient partiesClient, IResourceRegistryClient resourceRegistryClient, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap)
    {
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

    private static bool CheckIfInstanceIsDelegable(List<Right> delegableRights, RightInternal rightToDelegate)
    {
        return delegableRights.Exists(delegableRight => InstanceRightComparesEqualToDelegableRight(delegableRight, rightToDelegate));
    }

    private static bool InstanceRightComparesEqualToDelegableRight(Right right, RightInternal instanceRight)
    {
        if (right.Action.Value != instanceRight.Action.ValueSpan.ToString())
        {
            return false;
        }

        foreach (var resourcePart in right.Resource)
        {
            bool valid = instanceRight.Resource.Exists(r => r.Value.PrefixSpan.ToString() == resourcePart.Id && r.Value.ValueSpan.ToString() == resourcePart.Value);

            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateAndGetSignificantResourcePartsFromResource(IEnumerable<UrnJsonTypeValue> input, out List<UrnJsonTypeValue> resource, string resourceTag)
    {
        resource = new List<UrnJsonTypeValue>();
        
        if (input == null || !input.Any())
        {
            return false;
        }

        string org = null, app = null, resourceRegistryId = null;
        int significantParts = 0;
        foreach (UrnJsonTypeValue urnJsonTypeValue in input)
        {
            if (urnJsonTypeValue.HasValue)
            {
                switch (urnJsonTypeValue.Value.PrefixSpan.ToString())
                {
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute:
                        resource.Add(urnJsonTypeValue);
                        org = urnJsonTypeValue.Value.ValueSpan.ToString();
                        significantParts++;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute:
                        resource.Add(urnJsonTypeValue);
                        app = urnJsonTypeValue.Value.ValueSpan.ToString();
                        significantParts++;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute:
                        resource.Add(urnJsonTypeValue);
                        resourceRegistryId = urnJsonTypeValue.Value.ValueSpan.ToString();
                        significantParts++;
                        break;
                }
            }
        }

        if (org != null && app != null && resourceRegistryId == null && significantParts == 2)
        {
            return $"app_{org}_{app}" == resourceTag;
        }

        if (org == null && app == null && resourceRegistryId != null && significantParts == 1)
        {
            return resourceRegistryId == resourceTag;
        }

        return false;
    }

    private static void AddValidationErrorsForResourceInstance(ref ValidationErrorBuilder errors, IEnumerable<RightInternal> rights, string resourceid)
    {
        ValidateAndGetSignificantResourcePartsFromResource(rights.FirstOrDefault()?.Resource, out List<UrnJsonTypeValue> firstResource, resourceid);
        int counter = -1;

        foreach (RightInternal rightV2 in rights)
        {
            counter++;

            bool valid = ValidateAndGetSignificantResourcePartsFromResource(rightV2.Resource, out List<UrnJsonTypeValue> currentResource, resourceid);
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
    public async Task<Result<AppsInstanceDelegationResponse>> Delegate(AppsInstanceDelegationRequest appsInstanceDelegationRequest, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;

        // Fetch from and to from partyuuid
        (UuidType Type, Guid? Uuid) from = await TranslatePartyUuidToPersonOrganizationUuid(appsInstanceDelegationRequest.From);
        (UuidType Type, Guid? Uuid) to = await TranslatePartyUuidToPersonOrganizationUuid(appsInstanceDelegationRequest.To);

        if (from.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "From");
        }

        if (to.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "To");
        }

        // Validate Resource and instance 1. All rights include resource 2. All rights resource is identical to central value
        AddValidationErrorsForResourceInstance(ref errors, appsInstanceDelegationRequest.Rights, appsInstanceDelegationRequest.ResourceId);

        // Fetch rights valid for delegation
        ServiceResource resource = (await _resourceRegistryClient.GetResourceList(cancellationToken)).Find(r => r.Identifier == appsInstanceDelegationRequest.ResourceId);
        List<Right> delegableRights = null;

        if (resource == null)
        {
            errors.Add(ValidationErrors.InvalidResource, "appInstanceDelegationRequest.Resource");
        }
        else
        {
            RightQueryForApp resourceQuery = new RightQueryForApp { OwnerApp = appsInstanceDelegationRequest.PerformedBy, Resource = resource.AuthorizationReference };

            try
            {
                delegableRights = await _pip.GetDelegableRightsByApp(resourceQuery, cancellationToken);
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

        // Perform delegation
        DelegationHelper.TryGetPerformerFromAttributeMatches(appsInstanceDelegationRequest.PerformedBy, out string performedById, out UuidType performedByType);
        InstanceRight rulesToDelegate = new InstanceRight
        {
            FromUuid = (Guid)from.Uuid,
            FromType = from.Type,
            ToUuid = (Guid)to.Uuid,
            ToType = to.Type,
            PerformedBy = performedById,
            PerformedByType = performedByType,
            ResourceId = appsInstanceDelegationRequest.ResourceId,
            InstanceId = appsInstanceDelegationRequest.InstanceId,
            InstanceDelegationMode = appsInstanceDelegationRequest.InstanceDelegationMode
        };
        List<RightInternal> rightsAppCantDelegate = new List<RightInternal>();
        UrnJsonTypeValue instanceId = KeyValueUrn.CreateUnchecked($"{AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute}:{appsInstanceDelegationRequest.InstanceId}", AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute.Length + 1);
        
        foreach (RightInternal rightToDelegate in appsInstanceDelegationRequest.Rights)
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

        AppsInstanceDelegationResponse result = new()
        {
            From = appsInstanceDelegationRequest.From,
            To = appsInstanceDelegationRequest.To,
            ResourceId = appsInstanceDelegationRequest.ResourceId,
            InstanceId = appsInstanceDelegationRequest.InstanceId,
            InstanceDelegationMode = appsInstanceDelegationRequest.InstanceDelegationMode
        };

        List<InstanceRightDelegationResult> rights = await DelegateRights(rulesToDelegate, rightsAppCantDelegate, cancellationToken);

        result.Rights = rights;
        
        return result;
    }

    private async Task<List<InstanceRightDelegationResult>> DelegateRights(InstanceRight rulesToDelegate, List<RightInternal> rightsAppCantDelegate, CancellationToken cancellationToken)
    {
        List<InstanceRightDelegationResult> rights = new List<InstanceRightDelegationResult>();

        if (rulesToDelegate.InstanceRules.Count > 0)
        {
            InstanceRight delegationResult = await _pap.TryWriteInstanceDelegationPolicyRules(rulesToDelegate, cancellationToken);
            rights.AddRange(DelegationHelper.GetRightDelegationResultsFromInstanceRules(delegationResult));
        }

        if (rightsAppCantDelegate.Count > 0)
        {
            rights.AddRange(DelegationHelper.GetRightDelegationResultsFromFailedInternalRights(rightsAppCantDelegate));
        }

        return rights;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Get(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc/>
    public Task<Result<bool>> Revoke(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}