using System.ComponentModel.DataAnnotations;
using System.Linq;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Utilities;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly ILogger<IDelegationsService> _logger;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IResourceAdministrationPoint _resourceAdministrationPoint;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IRegister _registerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="logger">handler for logger</param>
        /// <param name="delegationRepository">delegation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="resourceAdministrationPoint">handler for resource registry</param>
        /// <param name="pip">Service implementation for policy information point</param>
        /// <param name="pap">Service implementation for policy administration point</param>
        /// <param name="registerService">Service implementation for register lookup</param>
        public DelegationsService(ILogger<IDelegationsService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IResourceAdministrationPoint resourceAdministrationPoint, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap, IRegister registerService)
        {
            _logger = logger;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _resourceAdministrationPoint = resourceAdministrationPoint;
            _pip = pip;
            _pap = pap;
            _registerService = registerService;
        }

        /// <inheritdoc/>
        public async Task<DelegationActionResult> MaskinportenDelegation(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation)
        {
            (DelegationActionResult result, string resourceRegistryId, Party fromParty, Party toParty) = await ValidateMaskinportenDelegationModel(DelegationActionType.Delegation, delegation);
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
        public async Task<List<Delegation>> GetOfferedMaskinportenSchemaDelegations(AttributeMatch party)
        {
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute)
            {
                throw new ArgumentException($"Maskinporten schema delegations is not supported between persons. Invalid argument: {party.Id}");
            }

            int offeredByPartyId = 0;
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                offeredByPartyId = await _contextRetrievalService.GetPartyId(party.Value);
            }
            else if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute && (!int.TryParse(party.Value, out offeredByPartyId) || offeredByPartyId == 0))
            {
                throw new ArgumentException($"The specified PartyId is not a valid. Invalid argument: {party.Value}");
            }

            return await GetOfferedDelegations(offeredByPartyId, ResourceType.MaskinportenSchema);
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetReceivedMaskinportenSchemaDelegations(AttributeMatch party)
        {
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute)
            {
                throw new ArgumentException($"Maskinporten schema delegations is not supported between persons. Invalid argument: {party.Id}");
            }

            int coveredByPartyId = 0;
            if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                coveredByPartyId = await _contextRetrievalService.GetPartyId(party.Value);
            }
            else if (party.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute && (!int.TryParse(party.Value, out coveredByPartyId) || coveredByPartyId == 0))
            {
                throw new ArgumentException($"The specified PartyId is not a valid. Invalid argument: {party.Value}");
            }

            return await GetReceivedDelegations(coveredByPartyId, ResourceType.MaskinportenSchema);
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetMaskinportenSchemaDelegations(string supplierOrg, string consumerOrg, string scope)
        {
            int consumerPartyId = string.IsNullOrEmpty(consumerOrg) ? 0 : await _contextRetrievalService.GetPartyId(consumerOrg);
            int supplierPartyId = string.IsNullOrEmpty(supplierOrg) ? 0 : await _contextRetrievalService.GetPartyId(supplierOrg);

            if (!RegexUtil.IsValidMaskinportenScope(scope))
            {
                throw new ArgumentException("Scope is not well formatted");
            }

            return await GetAllMaskinportenSchemaDelegations(supplierPartyId, consumerPartyId, scope);
        }

        /// <inheritdoc/>
        public async Task<DelegationActionResult> RevokeMaskinportenDelegation(int authenticatedUserId, DelegationLookup delegation)
        {
            (DelegationActionResult result, string resourceRegistryId, Party fromParty, Party toParty) = await ValidateMaskinportenDelegationModel(DelegationActionType.Revoke, delegation);
            if (!result.IsValid)
            {
                return result;
            }

            List<RequestToDelete> policiesToDelete = DelegationHelper.GetRequestToDeleteResourceRegistryService(authenticatedUserId, resourceRegistryId, fromParty.PartyId, toParty.PartyId);

            await _pap.TryDeleteDelegationPolicies(policiesToDelete);
            return result;
        }

        private async Task<(DelegationActionResult Result, string ResourceRegistryId, Party FromParty, Party ToParty)> ValidateMaskinportenDelegationModel(DelegationActionType delegationAction, DelegationLookup delegation)
        {
            DelegationActionResult result = new DelegationActionResult { To = delegation.To, Rights = delegation.Rights };

            // Verify request is for single resource registry id
            if (delegation.Rights?.Count != 1)
            {
                result.Errors.Add("Rights", "This operation only support requests specifying a single right identifying a Maskinporten schema resource registered in the Altinn Resource Registry");
                return (result, string.Empty, null, null);
            }

            Right right = delegation.Rights.First();
            DelegationHelper.TryGetResourceFromAttributeMatch(right.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string _, out string _);

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
                fromParty = await _registerService.GetOrganisation(fromOrgNo);
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
                toParty = await _registerService.GetOrganisation(toOrgNo);
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

            List<int> parties = new List<int>();
            foreach (int party in delegationChanges.Select(d => d.CoveredByPartyId).Where(c => c != null).OfType<int>())
            {
                parties.Add(party);
            }

            List<ServiceResource> resources = new List<ServiceResource>();
            List<Tuple<string, string>> resourceIds;
            resourceIds = delegationChanges.Select(d => Tuple.Create(d.ResourceId, d.ResourceType)).ToList();

            resources = await _resourceAdministrationPoint.GetResources(resourceIds);

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            
            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                delegation.CoveredByName = partyInfo?.Name;
                delegation.CoveredByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.PerformedByPartyId = delegationChange.PerformedByPartyId;
                delegation.Created = delegationChange.Created.Value;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.ResourceReferences = resource.ResourceReferences;
                delegation.ResourceType = resource.ResourceType;
                delegation.HasCompetentAuthority = resource.HasCompetentAuthority;
                delegation.Description = resource.Description;
                delegation.RightDescription = resource.RightDescription;
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetReceivedDelegations(int coveredByPartyId, ResourceType resourceType)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId.SingleToList(), resourceTypes: resourceType.SingleToList());

            if (delegationChanges?.Count == 0)
            {
                return delegations;
            }

            List<int> parties = new List<int>();
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();

            List<ServiceResource> resources = new List<ServiceResource>();
            List<Tuple<string, string>> resourceIds;
            resourceIds = delegationChanges.Select(d => Tuple.Create(d.ResourceId, d.ResourceType)).ToList();
            resources = await _resourceAdministrationPoint.GetResources(resourceIds);

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);

            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                delegation.OfferedByName = partyInfo?.Name;
                delegation.OfferedByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.Created = delegationChange.Created.Value;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.ResourceReferences = resource.ResourceReferences;
                delegation.ResourceType = resource.ResourceType;
                delegation.HasCompetentAuthority = resource.HasCompetentAuthority;
                delegation.Description = resource.Description;
                delegation.RightDescription = resource.RightDescription;
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetAllMaskinportenSchemaDelegations(int supplierPartyId, int consumerPartyId, string scopes)
        {
            List<Delegation> delegations = new List<Delegation>();

            List<ServiceResource> resources = await _resourceAdministrationPoint.GetResources(scopes);
            if (resources.Count == 0)
            {
                return delegations;
            }

            List<DelegationChange> delegationChanges = await _delegationRepository.GetResourceRegistryDelegationChanges(resources.Select(d => d.Identifier).ToList(), supplierPartyId, consumerPartyId, ResourceType.MaskinportenSchema);
            if (delegationChanges.Count == 0)
            {
                return delegations;
            }

            List<int> parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();
            parties.AddRange(delegationChanges.Select(d => d.CoveredByPartyId).Select(ds => Convert.ToInt32(ds)).ToList());

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            
            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                Party coveredByPartyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                delegation.OfferedByName = partyInfo?.Name;
                delegation.OfferedByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByName = coveredByPartyInfo?.Name;
                delegation.CoveredByOrganizationNumber = Convert.ToInt32(coveredByPartyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.Created = delegationChange.Created ?? DateTime.MinValue;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.ResourceReferences = resource.ResourceReferences;
                delegation.ResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }
    }
}
