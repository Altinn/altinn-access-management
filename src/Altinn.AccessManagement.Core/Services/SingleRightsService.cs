﻿using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class SingleRightsService : ISingleRightsService
    {
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRightsService"/> class.
        /// </summary>
        /// <param name="delegationRepository">delegation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="pip">Service implementation for policy information point</param>
        /// <param name="pap">Service implementation for policy administration point</param>
        public SingleRightsService(IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap)
        {
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _pip = pip;
            _pap = pap;
        }

        /// <inheritdoc/>
        public async Task<DelegationCheckResult> RightsDelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, RightsDelegationCheckRequest request)
        {
            (DelegationCheckResult result, ServiceResource resource, Party fromParty) = await ValidateRightDelegationStatusRequest(request);
            if (!result.IsValid)
            {
                return result;
            }

            DelegationHelper.TryGetResourceFromAttributeMatch(request.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out string serviceCode, out string serviceEditionCode);
            if (resource.ResourceType == ResourceType.Altinn2Service)
            {
                result.Errors.Add("right[0].Resource", $"Altinn apps and Altinn 2 services are not yet supported. {resource}"); //// ToDo: Update when support exists
                return result;
            }

            // Get all delegable rights
            RightsQuery rightsQuery = RightsHelper.GetRightsQuery(authenticatedUserId, fromParty.PartyId, resourceRegistryId, org, app);
            List<Right> allDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true, returnAllPolicyRights: true);
            if (allDelegableRights == null || allDelegableRights.Count == 0)    
            {
                result.Errors.Add("right[0].Resource", $"No delegable rights could be found for the resource: {resource}");
                return result;
            }

            if (allDelegableRights.Exists(r => r.RightSources.Exists(rs => rs.MinimumAuthenticationLevel > authenticatedUserAuthlevel)))
            {
                result.Errors.Add("right[0].Resource", $"Authenticated user does not meet the required security level requirement for resource: {resource}"); //// ToDo: convert to status?
                return result;
            }

            // Build result model with status
            foreach (Right right in allDelegableRights)
            {
                RightDelegationCheckResult rightDelegationStatus = new RightDelegationCheckResult
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
        public Task<DelegationActionResult> DelegateRights(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<Delegation>> GetOfferedRightsDelegations(AttributeMatch party)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<Delegation>> GetReceivedRightsDelegations(AttributeMatch party)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<DelegationActionResult> RevokeRightsDelegation(int authenticatedUserId, DelegationLookup delegation)
        {
            throw new NotImplementedException();
        }

        private async Task<(DelegationCheckResult Result, ServiceResource Resource, Party FromParty)> ValidateRightDelegationStatusRequest(RightsDelegationCheckRequest request)
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
            if (resource == null || !resource.Delegable)
            {
                result.Errors.Add("right[0].Resource", $"The resource does not exist or is not available for delegation");
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
    }
}