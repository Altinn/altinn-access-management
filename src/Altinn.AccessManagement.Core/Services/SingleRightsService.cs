﻿using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class SingleRightsService : ISingleRightsService
    {
        private readonly ILogger<ISingleRightsService> _logger;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IResourceAdministrationPoint _resourceAdministrationPoint;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRightsService"/> class.
        /// </summary>
        /// <param name="logger">handler for logger</param>
        /// <param name="delegationRepository">delegation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="resourceAdministrationPoint">handler for resource registry</param>
        /// <param name="pip">Service implementation for policy information point</param>
        /// <param name="pap">Service implementation for policy administration point</param>
        public SingleRightsService(ILogger<ISingleRightsService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IResourceAdministrationPoint resourceAdministrationPoint, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap)
        {
            _logger = logger;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _resourceAdministrationPoint = resourceAdministrationPoint;
            _pip = pip;
            _pap = pap;
        }

        /// <inheritdoc/>
        public async Task<DelegationCheckResult> RightsDelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, RightDelegationStatusRequest request)
        {
            (DelegationCheckResult result, ServiceResource resource, Party fromParty) = await ValidateRightDelegationStatusRequest(request);
            if (!result.IsValid)
            {
                return result;
            }

            if (resource.ResourceType == ResourceType.Altinn2Service || resource.ResourceType == ResourceType.AltinnApp) //// ToDo: Remove when support exists
            {
                result.Errors.Add("right[0].Resource", $"Altinn apps and Altinn 2 services are not yet supported. {resource}");
                return result;
            }

            // Get all delegable rights
            RightsQuery rightsQuery = RightsHelper.GetRightsQueryForResourceRegistryService(authenticatedUserId, resource.Identifier, fromParty.PartyId);
            List<Right> allDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true, returnAllPolicyRights: true);
            if (allDelegableRights == null || allDelegableRights.Count == 0)
            {
                result.Errors.Add("right[0].Resource", $"No delegable rights could be found for the resource: {resource}");
                return result;
            }

            if (allDelegableRights.Any(r => r.RightSources.Any(rs => rs.MinimumAuthenticationLevel > authenticatedUserAuthlevel)))
            {
                result.Errors.Add("right[0].Resource", $"Authenticated user does not meet the required security level requirement for resource: {resource}"); //// ToDo: convert to status?
                return result;
            }

            // Build result model with status
            foreach (Right right in allDelegableRights)
            {
                RightDelegationStatus rightDelegationStatus = new RightDelegationStatus
                {
                    RightKey = right.RightKey,
                    Resource = right.Resource,
                    Action = right.Action,
                    Status = (right.CanDelegate.HasValue && right.CanDelegate.Value) ? DelegableStatus.Delegable : DelegableStatus.NotDelegable
                };

                rightDelegationStatus.Details = RightsHelper.AnalyzeDelegationAccessReason(right);

                result.RightsStatus.Add(rightDelegationStatus);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DelegationActionResult> DelegateRights(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation)
        {
            throw new NotImplementedException();

            (DelegationActionResult result, string resourceRegistryId, Party fromParty, Party toParty) = await ValidateDelegationLookupModel(DelegationActionType.Delegation, delegation);
            if (!result.IsValid)
            {
                return result;
            }

            // Verify authenticated users delegable rights
            RightsQuery rightsQuery = RightsHelper.GetRightsQueryForResourceRegistryService(authenticatedUserId, resourceRegistryId, fromParty.PartyId);
            List<Right> usersDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true);
            if (usersDelegableRights == null || usersDelegableRights.Count == 0)
            {
                result.Errors.Add("right[0].Resource", $"Authenticated user does not have any delegable rights for the resource: {resourceRegistryId}");
                return result;
            }

            if (usersDelegableRights.Any(r => r.RightSources.Any(rs => rs.MinimumAuthenticationLevel > authenticatedUserAuthlevel)))
            {
                result.Errors.Add("right[0].Resource", $"Authenticated user does not meet the required security level requirement for resource: {resourceRegistryId}");
                return result;
            }

            // Perform delegation
            List<Rule> rulesToDelegate = new List<Rule>();
            foreach (Right rightToDelegate in usersDelegableRights)
            {
                rulesToDelegate.Add(new Rule
                {
                    DelegatedByUserId = authenticatedUserId,
                    OfferedByPartyId = fromParty.PartyId,
                    CoveredBy = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = toParty.PartyId.ToString() } },
                    Resource = rightToDelegate.Resource,
                    Action = rightToDelegate.Action
                });
            }

            List<Rule> delegationResult = await _pap.TryWriteDelegationPolicyRules(rulesToDelegate);
            if (delegationResult.All(r => r.CreatedSuccessfully))
            {
                result.Rights = usersDelegableRights;
                return await Task.FromResult(result);
            }
            else if (delegationResult.Any(r => r.CreatedSuccessfully))
            {
                // Partial delegation of rules should not really be possible. Return success but log error?
                _logger.LogError("One or more rules could not be delegated.\n{result}", delegationResult);
                result.Rights = usersDelegableRights;
                return await Task.FromResult(result);
            }

            result.Errors.Add("Rights", "Delegation was not able complete");
            return result;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetOfferedRightsDelegations(AttributeMatch party)
        {
            throw new NotImplementedException();

            int offeredByPartyId = 0;
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                Party offeredByParty = await _contextRetrievalService.GetPartyForOrganization(party.Value);
                offeredByPartyId = offeredByParty.PartyId;
            }
            //// ToDo add SSN support
            else if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute && (!int.TryParse(party.Value, out offeredByPartyId) || offeredByPartyId == 0))
            {
                throw new ArgumentException($"The specified PartyId is not a valid. Invalid argument: {party.Value}");
            }

            return await GetOfferedDelegations(offeredByPartyId, ~ResourceType.MaskinportenSchema);
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetReceivedRightsDelegations(AttributeMatch party)
        {
            throw new NotImplementedException();

            int coveredByPartyId = 0;
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                Party coveredByParty = await _contextRetrievalService.GetPartyForOrganization(party.Value);
                coveredByPartyId = coveredByParty.PartyId;
            }
            //// ToDo add SSN support
            else if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute && (!int.TryParse(party.Value, out coveredByPartyId) || coveredByPartyId == 0))
            {
                throw new ArgumentException($"The specified PartyId is not a valid. Invalid argument: {party.Value}");
            }

            return await GetReceivedDelegations(coveredByPartyId, ~ResourceType.MaskinportenSchema);
        }

        /// <inheritdoc/>
        public async Task<DelegationActionResult> RevokeRightsDelegation(int authenticatedUserId, DelegationLookup delegation)
        {
            throw new NotImplementedException();

            (DelegationActionResult result, string resourceRegistryId, Party fromParty, Party toParty) = await ValidateDelegationLookupModel(DelegationActionType.Revoke, delegation);
            if (!result.IsValid)
            {
                return result;
            }

            List<RequestToDelete> policiesToDelete = DelegationHelper.GetRequestToDeleteResourceRegistryService(authenticatedUserId, resourceRegistryId, fromParty.PartyId, toParty.PartyId);
            
            //// ToDo: Add support for App and A2 Service

            await _pap.TryDeleteDelegationPolicies(policiesToDelete);
            return result;
        }

        private async Task<(DelegationCheckResult Result, ServiceResource Resource, Party FromParty)> ValidateRightDelegationStatusRequest(RightDelegationStatusRequest request)
        {
            DelegationCheckResult result = new DelegationCheckResult { From = request.From, RightsStatus = new() };

            DelegationHelper.TryGetResourceFromAttributeMatch(request.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out string serviceCode, out string serviceEditionCode);

            if (resourceMatchType == ResourceAttributeMatchType.None)
            {
                result.Errors.Add("right[0].Resource", $"The specified resource is not recognized. The operation only support requests for a single resource from either the Altinn Resource Registry identified by using the {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} attribute id, Altinn Apps identified by using {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute}, or Altinn 2 services identified by using {AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceCodeAttribute}");
                return (result, null, null);
            }

            // Verify resource is valid
            ServiceResource resource = await _contextRetrievalService.GetResourceFromResourceList(resourceRegistryId, org, app, serviceCode, serviceEditionCode);
            if (resource == null || (resource.IsComplete.HasValue && !resource.IsComplete.Value))
            {
                result.Errors.Add("right[0].Resource", $"The resource does not exist or is not complete and available for delegation");
                return (result, resource, null);
            }

            if (!resource.Delegable)
            {
                result.Errors.Add("right[0].Resource", $"The resource: {resource}, is not available for delegation");
                return (result, resource, null);
            }

            // Verify and get From reportee party of the delegation
            Party fromParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(request.From, out string fromOrgNo))
            {
                fromParty = await _contextRetrievalService.GetPartyForOrganization(fromOrgNo);
            }
            else if (DelegationHelper.TryGetSocialSecurityNumberAttributeMatch(request.From, out string fromSsn))
            {
                fromParty = await _contextRetrievalService.GetPartyForPerson(fromSsn); //// ToDo: make SSN party lookup. Can we do this based on SSN alone or do we need last name?
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(request.From, out int fromPartyId))
            {
                List<Party> fromPartyLookup = await _contextRetrievalService.GetPartiesAsync(fromPartyId.SingleToList());
                fromParty = fromPartyLookup.FirstOrDefault();
            }

            if (fromParty == null)
            {
                result.Errors.Add("From", $"Could not identify the From party. Please try again."); //// ToDo: This shouldn't really happen, as to get here the request must have been authorized for the From reportee, but the register integration could fail.
                return (result, resource, null);
            }

            return (result, resource, fromParty);
        }

        private async Task<(DelegationActionResult Result, string ResourceRegistryId, Party FromParty, Party ToParty)> ValidateDelegationLookupModel(DelegationActionType delegationAction, DelegationLookup delegation)
        {
            DelegationActionResult result = new DelegationActionResult { To = delegation.To, Rights = delegation.Rights };

            //// ToDo: Update from MaskinportenSchema delegation to SingleRight delegation

            // Verify request is for single resource registry id
            if (delegation.Rights?.Count != 1)
            {
                result.Errors.Add("Rights", "This operation only support requests specifying a single right identifying a Maskinporten schema resource registered in the Altinn Resource Registry");
                return (result, string.Empty, null, null);
            }

            Right right = delegation.Rights.First();
            DelegationHelper.TryGetResourceFromAttributeMatch(right.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string _, out string _, out string _, out string _);

            if (resourceMatchType != ResourceAttributeMatchType.ResourceRegistry)
            {
                result.Errors.Add("right[0].Resource", $"This operation only support requests for resources from the Altinn Resource Registry using the {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} attribute id");
                return (result, resourceRegistryId, null, null);
            }

            // Verify resource registry id is a valid MaskinportenSchema
            ServiceResource resource = await _contextRetrievalService.GetResource(resourceRegistryId);
            if (resource == null || (delegationAction == DelegationActionType.Delegation && resource.IsComplete.HasValue && !resource.IsComplete.Value))
            {
                result.Errors.Add("right[0].Resource", $"The resource: {resourceRegistryId}, does not exist or is not complete and available for delegation");
                return (result, resourceRegistryId, null, null);
            }

            if (resource.ResourceType != ResourceType.MaskinportenSchema)
            {
                result.Errors.Add("right[0].Resource", $"This operation only support requests for Maskinporten schema resources. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
                return (result, resourceRegistryId, null, null);
            }

            // Verify and get From reportee party of the delegation
            Party fromParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.From, out string fromOrgNo))
            {
                fromParty = await _contextRetrievalService.GetPartyForOrganization(fromOrgNo);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.From, out int fromPartyId))
            {
                List<Party> fromPartyLookup = await _contextRetrievalService.GetPartiesAsync(fromPartyId.SingleToList());
                fromParty = fromPartyLookup.FirstOrDefault();
            }

            if (fromParty == null || fromParty.PartyTypeName != PartyType.Organisation)
            {
                result.Errors.Add("From", $"Maskinporten schema delegation can only be delegated from a valid organization (identified by either {AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute} or {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute} attribute id). Invalid value: {delegation.From.FirstOrDefault()?.Value}");
                return (result, resourceRegistryId, null, null);
            }

            // Verify and get To recipient party of the delegation
            Party toParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.To, out string toOrgNo))
            {
                toParty = await _contextRetrievalService.GetPartyForOrganization(toOrgNo);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.To, out int toPartyId))
            {
                List<Party> toPartyLookup = await _contextRetrievalService.GetPartiesAsync(toPartyId.SingleToList());
                toParty = toPartyLookup.FirstOrDefault();
            }

            if (toParty == null || toParty.PartyTypeName != PartyType.Organisation)
            {
                result.Errors.Add("To", $"Maskinporten schema delegation can only be delegated to a valid organization (identified by either {AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute} or {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute} attribute id). Invalid value: {delegation.To.FirstOrDefault()?.Value}");
                return (result, resourceRegistryId, null, null);
            }

            return (result, resourceRegistryId, fromParty, toParty);
        }

        private async Task<List<Delegation>> GetOfferedDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetOfferedResourceRegistryDelegations(offeredByPartyId, resourceTypes: resourceType.SingleToList());

            if (delegationChanges?.Count == 0)
            {
                return delegations;
            }

            return await BuildDelegationsResponse(delegationChanges);
        }

        private async Task<List<Delegation>> GetReceivedDelegations(int coveredByPartyId, ResourceType resourceType)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId.SingleToList(), resourceTypes: resourceType.SingleToList());

            if (delegationChanges?.Count == 0)
            {
                return delegations;
            }

            return await BuildDelegationsResponse(delegationChanges);
        }

        private async Task<List<Delegation>> BuildDelegationsResponse(List<DelegationChange> delegationChanges, List<ServiceResource> resources = null)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<int> parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();
            parties.AddRange(delegationChanges.Select(d => d.CoveredByPartyId).Select(ds => Convert.ToInt32(ds)).ToList());

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);

            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Party offeredByParty = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                Party coveredByParty = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                ServiceResource resource = resources?.FirstOrDefault(r => r.Identifier == delegationChange.ResourceId);
                delegations.Add(BuildDelegationModel(delegationChange, offeredByParty, coveredByParty, resource));
            }

            return delegations;
        }

        private static Delegation BuildDelegationModel(DelegationChange delegationChange, Party offeredByParty, Party coveredByParty, ServiceResource resource)
        {
            ResourceType resourceType = Enum.TryParse(delegationChange.ResourceType, true, out ResourceType type) ? type : ResourceType.Default;
            Delegation delegation = new Delegation
            {
                OfferedByPartyId = delegationChange.OfferedByPartyId,
                OfferedByName = offeredByParty?.Name,
                OfferedByOrganizationNumber = offeredByParty?.OrgNumber,
                CoveredByPartyId = delegationChange.CoveredByPartyId,
                CoveredByName = coveredByParty?.Name,
                CoveredByOrganizationNumber = coveredByParty?.OrgNumber,
                PerformedByUserId = delegationChange.PerformedByUserId,
                PerformedByPartyId = delegationChange.PerformedByPartyId,
                Created = delegationChange.Created ?? DateTime.MinValue,
                ResourceId = delegationChange.ResourceId,
                ResourceType = resourceType
            };

            if (resource != null)
            {
                delegation.ResourceTitle = resource?.Title;
                delegation.Description = resource?.Description;
                delegation.RightDescription = resource?.RightDescription;
                delegation.ResourceReferences = resource?.ResourceReferences;
                delegation.HasCompetentAuthority = resource?.HasCompetentAuthority;
            }

            return delegation;
        }
    }
}
