﻿using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class SingleRightsService : ISingleRightsService
    {
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IAltinn2RightsClient _altinn2RightsClient;
        private readonly IProfileClient _profile;
        private readonly IUserProfileLookupService _profileLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRightsService"/> class.
        /// </summary>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="pip">Service implementation for policy information point</param>
        /// <param name="pap">Service implementation for policy administration point</param>
        /// <param name="altinn2RightsClient">SBL Bridge client implementation for rights operations on Altinn 2 services</param>
        /// <param name="profile">Client implementation for getting user profile</param>
        /// <param name="profileLookup">Service implementation for lookup of userprofile with lastname verification</param>
        public SingleRightsService(IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap, IAltinn2RightsClient altinn2RightsClient, IProfileClient profile, IUserProfileLookupService profileLookup)
        {
            _contextRetrievalService = contextRetrievalService;
            _pip = pip;
            _pap = pap;
            _altinn2RightsClient = altinn2RightsClient;
            _profile = profile;
            _profileLookup = profileLookup;
        }

        /// <inheritdoc/>
        public async Task<DelegationCheckResponse> RightsDelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, RightsDelegationCheckRequest request)
        {
            (DelegationCheckResponse result, ServiceResource resource, Party fromParty) = await ValidateRightDelegationCheckRequest(request);
            if (!result.IsValid)
            {
                return result;
            }

            RightsQuery rightsQuery;
            DelegationHelper.TryGetResourceFromAttributeMatch(request.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out string serviceCode, out string serviceEditionCode);
            if (resource.ResourceType == ResourceType.Altinn2Service)
            {
                return await _altinn2RightsClient.PostDelegationCheck(authenticatedUserId, fromParty.PartyId, serviceCode, serviceEditionCode);
            }

            rightsQuery = RightsHelper.GetRightsQuery(authenticatedUserId, fromParty.PartyId, resourceRegistryId, org, app);

            List<Right> allDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true, returnAllPolicyRights: true);
            if (allDelegableRights == null || allDelegableRights.Count == 0)
            {
                result.Errors.Add("right[0].Resource", $"No delegable rights could be found for the resource: {resource}");
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

                result.RightDelegationCheckResults.Add(rightDelegationStatus);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DelegationActionResult> DelegateRights(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation)
        {
            (DelegationActionResult result, ServiceResource resource, Party fromParty, List<AttributeMatch> to) = await ValidateDelegationLookupModel(DelegationActionType.Delegation, delegation, authenticatedUserId);
            if (!result.IsValid)
            {
                return result;
            }

            // Altinn 2 service delegation is handled by SBL Bridge
            DelegationHelper.TryGetResourceFromAttributeMatch(resource.AuthorizationReference, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out string _, out string _);
            if (resourceMatchType == ResourceAttributeMatchType.Altinn2Service)
            {
                SblRightDelegationRequest sblRightDelegationRequest = new SblRightDelegationRequest { To = to.FirstOrDefault(), Rights = delegation.Rights };
                DelegationActionResult sblResult = await _altinn2RightsClient.PostDelegation(authenticatedUserId, fromParty.PartyId, sblRightDelegationRequest);

                sblResult.To = result.To; // Set result.To to match original input
                return sblResult;
            }

            // Verify authenticated users delegable rights
            RightsQuery rightsQuery = RightsHelper.GetRightsQuery(authenticatedUserId, fromParty.PartyId, resourceRegistryId, org, app);
            List<Right> usersDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true);
            if (usersDelegableRights == null || usersDelegableRights.Count == 0)
            {
                result.Errors.Add("right[0].Resource", $"Authenticated user does not have any delegable rights for the resource: {resourceRegistryId}");
                return result;
            }

            // Perform delegation
            List<Rule> rulesToDelegate = new List<Rule>();
            List<Right> rightsUserCantDelegate = new List<Right>();
            foreach (Right rightToDelegate in delegation.Rights)
            {
                if (usersDelegableRights.Contains(rightToDelegate))
                {
                    rulesToDelegate.Add(new Rule
                    {
                        DelegatedByUserId = authenticatedUserId,
                        OfferedByPartyId = fromParty.PartyId,
                        CoveredBy = to,
                        Resource = rightToDelegate.Resource,
                        Action = rightToDelegate.Action
                    });
                }
                else
                {
                    rightsUserCantDelegate.Add(rightToDelegate);
                }
            }

            List<Rule> delegationResult = await _pap.TryWriteDelegationPolicyRules(rulesToDelegate);
            result.Rights = DelegationHelper.GetRightDelegationResultsFromRules(delegationResult);

            if (rightsUserCantDelegate.Any())
            {
                result.Rights.AddRange(DelegationHelper.GetRightDelegationResultsFromFailedRights(rightsUserCantDelegate));
            }

            return await Task.FromResult(result);
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

        private async Task<(DelegationCheckResponse Result, ServiceResource Resource, Party FromParty)> ValidateRightDelegationCheckRequest(RightsDelegationCheckRequest request)
        {
            DelegationCheckResponse result = new DelegationCheckResponse { From = request.From, RightDelegationCheckResults = new() };

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

            if (resource.ResourceType == ResourceType.MaskinportenSchema)
            {
                result.Errors.Add("right[0].Resource", $"This operation does not support MaskinportenSchema resources. Please use the MaskinportenSchema DelegationCheck API. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
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
                // ToDo: Can we do this based on SSN alone for the Reportee or do we need last name?
                fromParty = await _contextRetrievalService.GetPartyForPerson(fromSsn);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(request.From, out int fromPartyId))
            {
                List<Party> fromPartyLookup = await _contextRetrievalService.GetPartiesAsync(fromPartyId.SingleToList());
                fromParty = fromPartyLookup.FirstOrDefault();
            }

            if (fromParty == null)
            {
                // This shouldn't really happen, as to get here the request must have been authorized for the From reportee, but the register integration could fail.
                result.Errors.Add("From", $"Could not identify the From party. Please try again.");
                return (result, resource, null);
            }

            return (result, resource, fromParty);
        }

        private async Task<(DelegationActionResult Result, ServiceResource Resource, Party FromParty, List<AttributeMatch> To)> ValidateDelegationLookupModel(DelegationActionType delegationAction, DelegationLookup delegation, int authenticatedUserId)
        {
            DelegationActionResult result = new DelegationActionResult { From = delegation.From, To = delegation.To, Rights = new List<RightDelegationResult>() };

            if (delegation.Rights.Count == 0)
            {
                result.Errors.Add("Rights", "Request must contain at least one right to be delegated");
                return (result, null, null, null);
            }

            // Verify request is for single resource, app or Altinn 2 service
            if (delegation.Rights.Count > 1 && !ValidateAllRightsAreForTheSameResource(delegation.Rights))
            {
                result.Errors.Add("Rights", "Rights delegation only support requests where all rights are for the same resource, app or Altinn 2 service");
                return (result, null, null, null);
            }

            DelegationHelper.TryGetResourceFromAttributeMatch(delegation.Rights[0].Resource, out ResourceAttributeMatchType _, out string resourceRegistryId, out string org, out string app, out string serviceCode, out string serviceEditionCode);

            ServiceResource resource = await _contextRetrievalService.GetResourceFromResourceList(resourceRegistryId, org, app, serviceCode, serviceEditionCode);
            if (resource == null || !resource.Delegable)
            {
                result.Errors.Add("right[0].Resource", $"The resource does not exist or is not available for delegation");
                return (result, resource, null, null);
            }

            if (resource.ResourceType == ResourceType.MaskinportenSchema)
            {
                result.Errors.Add("right[0].Resource", $"This operation does not support delegations for MaskinportenSchema resources. Please use the MaskinportenSchema Delegations API. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
                return (result, resource, null, null);
            }

            // Verify and get From reportee party of the delegation
            Party fromParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.From, out string fromOrgNo))
            {
                fromParty = await _contextRetrievalService.GetPartyForOrganization(fromOrgNo);
            }
            else if (DelegationHelper.TryGetSocialSecurityNumberAttributeMatch(delegation.From, out string fromSsn))
            {
                fromParty = await _contextRetrievalService.GetPartyForPerson(fromSsn);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.From, out int fromPartyId))
            {
                List<Party> fromPartyLookup = await _contextRetrievalService.GetPartiesAsync(fromPartyId.SingleToList());
                fromParty = fromPartyLookup.FirstOrDefault();
            }

            if (fromParty == null)
            {
                // This shouldn't really happen, as to get here the request must have been authorized for the From reportee, but the register integration could fail.
                result.Errors.Add("From", $"Could not identify the From party. Please try again.");
                return (result, resource, null, null);
            }

            // Verify and get To recipient party of the delegation
            Party toParty = null;
            UserProfile toUser = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.To, out string toOrgNo))
            {
                toParty = await _contextRetrievalService.GetPartyForOrganization(toOrgNo);
            }
            else if (DelegationHelper.TryGetSocialSecurityNumberAndLastNameAttributeMatch(delegation.To, out string toSsn, out string toLastName))
            {
                toUser = await _profileLookup.GetUserProfile(authenticatedUserId, new UserProfileLookup { Ssn = toSsn }, toLastName);
            }
            else if (DelegationHelper.TryGetUsernameAndLastNameAttributeMatch(delegation.To, out string toUsername, out toLastName))
            {
                toUser = await _profileLookup.GetUserProfile(authenticatedUserId, new UserProfileLookup { Username = toUsername }, toLastName);
            }
            else if (DelegationHelper.TryGetEnterpriseUserNameAttributeMatch(delegation.To, out string enterpriseUserName))
            {
                toUser = await _profile.GetUser(new UserProfileLookup { Username = enterpriseUserName });

                if (toUser != null && toUser.Party.PartyId != fromParty.PartyId)
                {
                    result.Errors.Add("To", $"Enterpriseuser either does not exist or does not belong to the From party and cannot be delegated to.");
                    return (result, resource, null, null);
                }
            }

            if (toParty == null && toUser == null)
            {
                result.Errors.Add("To", $"A distinct recipient party for the delegation, could not be identified by the supplied attributes. A recipient can be identified by either a single {AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute} or {AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName} attribute, or a combination of {AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.LastName} attributes, or {AltinnXacmlConstants.MatchAttributeIdentifiers.UserName} and {AltinnXacmlConstants.MatchAttributeIdentifiers.LastName} attributes.");
                return (result, resource, null, null);
            }

            // Verify delegation From and To is not the same party (with exception for Altinn 2 Enterprise users)
            if (fromParty.PartyId == toParty?.PartyId || (toUser != null && fromParty.PartyId == toUser.PartyId && toUser.Party.PartyTypeName != PartyType.Organisation))
            {
                result.Errors.Add("To", $"The From party and the To recipient are the same. Self-delegation is not supported as it serves no purpose.");
                return (result, resource, null, null);
            }

            // Build To AttributeMatch to be used for the delegation rules
            List<AttributeMatch> to = new List<AttributeMatch>();
            if (toParty != null)
            {
                to.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = toParty.PartyId.ToString() });
            }

            if (toUser != null)
            {
                to.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = toUser.UserId.ToString() });
            }

            return (result, resource, fromParty, to);
        }

        private static bool ValidateAllRightsAreForTheSameResource(List<Right> rights)
        {
            string firstResourceKey = string.Empty;
            foreach (Right right in rights)
            {
                DelegationHelper.TryGetResourceFromAttributeMatch(right.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out string serviceCode, out string serviceEditionCode);
                string currentResourceKey = $"{resourceMatchType}{resourceRegistryId}{org}{app}{serviceCode}{serviceEditionCode}";

                if (firstResourceKey == string.Empty)
                {
                    firstResourceKey = currentResourceKey;
                }

                if (firstResourceKey != currentResourceKey)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
