using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
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

        return DelegationHelper.GetUuidTypeAndValueFromParty(party);
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
    public async Task<Result<ResourceDelegationCheckResponse>> DelegationCheck(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default)
    {
        ResourceDelegationCheckResponse result = new ResourceDelegationCheckResponse() { From = null, ResourceRightDelegationCheckResults = new List<ResourceRightDelegationCheckResult>() };

        ValidationErrorBuilder errors = default;

        ServiceResource resource = (await _resourceRegistryClient.GetResourceList(cancellationToken)).Find(r => r.Identifier == request.ResourceId);
        List<Right> delegableRights = null;

        if (resource == null)
        {
            errors.Add(ValidationErrors.InvalidResource, "appInstanceDelegationRequest.Resource");
        }
        else
        {
            RightsQuery rightsQueryForApp = new RightsQuery
            {
                Type = RightsQueryType.AltinnApp,
                To = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceDelegationAttribute, Value = request.PerformedBy.ValueSpan.ToString() }.SingleToList(),
                From = resource.AuthorizationReference,
                Resource = resource
            };

            try
            {
                delegableRights = await _pip.GetDelegableRightsByApp(rightsQueryForApp, cancellationToken);
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

        foreach (Right right in delegableRights)
        {
            result.ResourceRightDelegationCheckResults.Add(new()
            {
                RightKey = right.RightKey,
                Resource = right.Resource.Select(r => r.ToKeyValueUrn()).ToList(),
                Action = ActionUrn.ActionId.Create(ActionIdentifier.CreateUnchecked(right.Action.Value)),
                Status = (right.CanDelegate.HasValue && right.CanDelegate.Value) ? DelegableStatus.Delegable : DelegableStatus.NotDelegable
            });
        }

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<Result<AppsInstanceDelegationResponse>> Delegate(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default)
    {
        (ValidationErrorBuilder Errors, InstanceRight RulesToHandle, List<RightInternal> RightsAppCantHandle) input = await SetUpDelegationOrRevokeRequest(request, cancellationToken);

        if (input.Errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        AppsInstanceDelegationResponse result = new()
        {
            From = request.From,
            To = request.To,
            ResourceId = request.ResourceId,
            InstanceId = request.InstanceId,
            InstanceDelegationMode = request.InstanceDelegationMode
        };

        List<InstanceRightDelegationResult> rights = await DelegateRights(input.RulesToHandle, input.RightsAppCantHandle, cancellationToken);
        result.Rights = rights;
        result = RemoveInstanceIdFromResourceForDelegationResponse(result);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<AppsInstanceRevokeResponse>> Revoke(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default)
    {
        (ValidationErrorBuilder Errors, InstanceRight RulesToHandle, List<RightInternal> RightsAppCantHandle) input = await SetUpDelegationOrRevokeRequest(request, cancellationToken);

        if (input.Errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        AppsInstanceRevokeResponse result = new()
        {
            From = request.From,
            To = request.To,
            ResourceId = request.ResourceId,
            InstanceId = request.InstanceId,
            InstanceDelegationMode = request.InstanceDelegationMode
        };

        List<InstanceRightRevokeResult> rights = await RevokeRights(input.RulesToHandle, input.RightsAppCantHandle, cancellationToken);
        result.Rights = rights;
        result = RemoveInstanceIdFromResourceForRevokeResponse(result);

        return result;
    }

    private async Task<(ValidationErrorBuilder Errors, InstanceRight RulesToHandle, List<RightInternal> RightsAppCantHandle)> SetUpDelegationOrRevokeRequest(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;
        List<RightInternal> rightsAppCantHandle = null;
        InstanceRight rulesToHandle = null;

        // Fetch from and to from partyuuid
        (UuidType Type, Guid? Uuid) from = await TranslatePartyUuidToPersonOrganizationUuid(request.From);
        (UuidType Type, Guid? Uuid) to = await TranslatePartyUuidToPersonOrganizationUuid(request.To);

        if (from.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "From");
        }

        if (to.Type == UuidType.NotSpecified)
        {
            errors.Add(ValidationErrors.InvalidPartyUrn, "To");
        }

        // Validate Resource and instance 1. All rights include resource 2. All rights resource is identical to central value
        AddValidationErrorsForResourceInstance(ref errors, request.Rights, request.ResourceId);

        // Fetch rights valid for delegation
        ServiceResource resource = (await _resourceRegistryClient.GetResourceList(cancellationToken)).Find(r => r.Identifier == request.ResourceId);
        List<Right> delegableRights = null;

        if (resource == null)
        {
            errors.Add(ValidationErrors.InvalidResource, "appInstanceDelegationRequest.Resource");
        }
        else
        {
            RightsQuery rightsQueryForApp = new RightsQuery
            {
                Type = RightsQueryType.AltinnApp,
                To = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceDelegationAttribute, Value = request.PerformedBy.ValueSpan.ToString() }.SingleToList(),
                From = resource.AuthorizationReference,
                Resource = resource
            };

            try
            {
                delegableRights = await _pip.GetDelegableRightsByApp(rightsQueryForApp, cancellationToken);
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

        if (!errors.IsEmpty)
        {
            return (errors, rulesToHandle, rightsAppCantHandle);
        }

        rightsAppCantHandle = [];

        rulesToHandle = new InstanceRight
        {
            FromUuid = (Guid)from.Uuid,
            FromType = from.Type,
            ToUuid = (Guid)to.Uuid,
            ToType = to.Type,
            PerformedBy = request.PerformedBy.ValueSpan.ToString(),
            PerformedByType = UuidType.Resource,
            ResourceId = request.ResourceId,
            InstanceId = request.InstanceId,
            InstanceDelegationMode = request.InstanceDelegationMode,
            InstanceDelegationSource = request.InstanceDelegationSource,
        };

        UrnJsonTypeValue instanceId = KeyValueUrn.CreateUnchecked($"{AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute}:{request.InstanceId}", AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute.Length + 1);

        foreach (RightInternal rightToHandle in request.Rights)
        {
            if (CheckIfInstanceIsDelegable(delegableRights, rightToHandle))
            {
                rightToHandle.Resource.Add(instanceId);
                rulesToHandle.InstanceRules.Add(new InstanceRule
                {
                    Resource = rightToHandle.Resource,
                    Action = rightToHandle.Action
                });
            }
            else
            {
                rightsAppCantHandle.Add(rightToHandle);
            }
        }

        return (errors, rulesToHandle, rightsAppCantHandle);
    }

    private async Task<List<InstanceRightRevokeResult>> RevokeRights(InstanceRight rulesToRevoke, List<RightInternal> rightsAppCantRevoke, CancellationToken cancellationToken)
    {
        List<InstanceRightRevokeResult> rights = new List<InstanceRightRevokeResult>();

        if (rulesToRevoke.InstanceRules.Count > 0)
        {
            InstanceRight delegationResult = await _pap.TryWriteInstanceRevokePolicyRules(rulesToRevoke, cancellationToken);
            rights.AddRange(DelegationHelper.GetRightRevokeResultsFromInstanceRules(delegationResult));
        }

        if (rightsAppCantRevoke.Count > 0)
        {
            rights.AddRange(DelegationHelper.GetRightRevokeResultsFromFailedInternalRights(rightsAppCantRevoke));
        }

        return rights;
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
    public async Task<Result<List<AppsInstanceDelegationResponse>>> Get(AppsInstanceGetRequest request, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;

        // Fetch rights valid for delegation
        ServiceResource resource = (await _resourceRegistryClient.GetResourceList(cancellationToken)).Find(r => r.Identifier == request.ResourceId);
        List<Right> delegableRights = null;

        if (resource == null)
        {
            errors.Add(ValidationErrors.InvalidResource, "request.Resource");
        }
        else
        {
            RightsQuery rightsQueryForApp = new RightsQuery
            {
                Type = RightsQueryType.AltinnApp,
                To = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceDelegationAttribute, Value = request.PerformingResourceId.ValueSpan.ToString() }.SingleToList(),
                From = resource.AuthorizationReference,
                Resource = resource
            };

            try
            {
                delegableRights = await _pip.GetDelegableRightsByApp(rightsQueryForApp, cancellationToken);
            }
            catch (ValidationException)
            {
                errors.Add(ValidationErrors.MissingPolicy, "request.Resource");
            }

            // The app must be able to do at least one delegation to be able to do GET call
            if (delegableRights == null || !delegableRights.Exists(r => r.CanDelegate.HasValue && r.CanDelegate.Value))
            {
                errors.Add(ValidationErrors.MissingDelegableRights, "request.Resource");
            }
        }

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        List<AppsInstanceDelegationResponse> result = await _pip.GetInstanceDelegations(request, cancellationToken);
        result = RemoveInstanceIdFromResourceForDelegationResponseList(result);
        return result;
    }

    private static AppsInstanceRevokeResponse RemoveInstanceIdFromResourceForRevokeResponse(AppsInstanceRevokeResponse input)
    {
        foreach (var right in input.Rights)
        {
            right.Resource.RemoveAll(r => r.HasValue && r.Value.PrefixSpan.ToString() == AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute);
        }

        return input;
    }

    private static List<AppsInstanceDelegationResponse> RemoveInstanceIdFromResourceForDelegationResponseList(List<AppsInstanceDelegationResponse> input)
    {
        foreach (AppsInstanceDelegationResponse item in input)
        {
            RemoveInstanceIdFromResourceForDelegationResponse(item);
        }

        return input;
    }

    private static AppsInstanceDelegationResponse RemoveInstanceIdFromResourceForDelegationResponse(AppsInstanceDelegationResponse input)
    {
        foreach (var right in input.Rights)
        {
            right.Resource.RemoveAll(r => r.HasValue && r.Value.PrefixSpan.ToString() == AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceInstanceAttribute);
        }

        return input;
    }
}
