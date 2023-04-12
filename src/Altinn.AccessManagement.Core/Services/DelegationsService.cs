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

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="logger">handler for logger</param>
        /// <param name="delegationRepository">delegation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="resourceAdministrationPoint">handler for resource registry</param>
        /// <param name="pip">Service implementation for policy information point</param>
        /// <param name="pap">Service implementation for policy administration point</param>
        public DelegationsService(ILogger<IDelegationsService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IResourceAdministrationPoint resourceAdministrationPoint, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap)
        {
            _logger = logger;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _resourceAdministrationPoint = resourceAdministrationPoint;
            _pip = pip;
            _pap = pap;
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
                Party offeredByParty = await _contextRetrievalService.GetParty(party.Value);
                offeredByPartyId = offeredByParty.PartyId;
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
                Party coveredByParty = await _contextRetrievalService.GetParty(party.Value);
                coveredByPartyId = coveredByParty.PartyId;
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
            int consumerPartyId = 0;
            if (!string.IsNullOrEmpty(consumerOrg))
            {
                Party consumerParty = await _contextRetrievalService.GetParty(consumerOrg);
                if (consumerParty == null)
                {
                    throw new ArgumentException($"The specified consumerOrg: {consumerOrg}, is not a valid organization number");
                }

                consumerPartyId = consumerParty.PartyId;
            }

            int supplierPartyId = 0;
            if (!string.IsNullOrEmpty(supplierOrg))
            {
                Party supplierParty = await _contextRetrievalService.GetParty(supplierOrg);
                if (supplierParty == null)
                {
                    throw new ArgumentException($"The specified supplierOrg: {supplierOrg}, is not a valid organization number");
                }

                supplierPartyId = supplierParty.PartyId;
            }            

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
                fromParty = await _contextRetrievalService.GetParty(fromOrgNo);
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
                toParty = await _contextRetrievalService.GetParty(toOrgNo);
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

            List<Tuple<string, string>> resourceIds = delegationChanges.Select(d => Tuple.Create(d.ResourceId, d.ResourceType)).ToList();
            List<ServiceResource> resources = await _resourceAdministrationPoint.GetResources(resourceIds);

            return await BuildDelegationsResponse(delegationChanges, resources);
        }

        private async Task<List<Delegation>> GetReceivedDelegations(int coveredByPartyId, ResourceType resourceType)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId.SingleToList(), resourceTypes: resourceType.SingleToList());

            if (delegationChanges?.Count == 0)
            {
                return delegations;
            }

            List<Tuple<string, string>> resourceIds = delegationChanges.Select(d => Tuple.Create(d.ResourceId, d.ResourceType)).ToList();
            List<ServiceResource> resources = await _resourceAdministrationPoint.GetResources(resourceIds);

            return await BuildDelegationsResponse(delegationChanges, resources);
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

            return await BuildDelegationsResponse(delegationChanges, resources);
        }

        private async Task<List<Delegation>> BuildDelegationsResponse(List<DelegationChange> delegationChanges, List<ServiceResource> resources)
        {
            List<Delegation> delegations = new List<Delegation>();
            List<int> parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();
            parties.AddRange(delegationChanges.Select(d => d.CoveredByPartyId).Select(ds => Convert.ToInt32(ds)).ToList());

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);

            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Party offeredByParty = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                Party coveredByParty = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegations.Add(BuildDelegationModel(delegationChange, offeredByParty, coveredByParty, resource));
            }

            return delegations;
        }

        private static Delegation BuildDelegationModel(DelegationChange delegationChange, Party offeredByParty, Party coveredByParty, ServiceResource resource)
        {
            return new Delegation
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
                ResourceType = resource?.ResourceType ?? ResourceType.Default,
                ResourceTitle = resource?.Title,
                Description = resource?.Description,
                RightDescription = resource?.RightDescription,
                ResourceReferences = resource?.ResourceReferences,
                HasCompetentAuthority = resource?.HasCompetentAuthority
            };
        }
    }
}
