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
        public async Task<DelegationOutput> MaskinportenDelegation(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationInput delegation)
        {
            DelegationOutput output = new DelegationOutput { To = delegation.To, Rights = delegation.Rights };

            // Verify delegation for single resource registry id
            if (delegation.Rights?.Count != 1)
            {
                output.Errors.Add("Rights", "Maskinporten schema delegation must specify only a single right identifying the Maskinporten schema resource, registered in the Resource Registry");
                return output;
            }

            Right right = delegation.Rights.First();
            DelegationHelper.TryGetResourceFromAttributeMatch(right.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string _, out string _);

            if (resourceMatchType != ResourceAttributeMatchType.ResourceRegistry)
            {
                output.Errors.Add("right[0].Resource", $"Maskinporten schema delegation only support delegation of resources from the Resource Registry using the {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} attribute id");
                return output;
            }

            // Verify resource registry id is a valid MaskinportenSchema
            ServiceResource resource = await _contextRetrievalService.GetResource(resourceRegistryId);
            if (resource == null || (resource.IsComplete.HasValue && !resource.IsComplete.Value))
            {
                output.Errors.Add("right[0].Resource", $"The resource: {resourceRegistryId}, does not exist or is not complete and available for delegation");
                return output;
            }

            if (resource.ResourceType != ResourceType.MaskinportenSchema)
            {
                output.Errors.Add("right[0].Resource", $"Maskinporten schema delegation can only be used to delegate maskinporten schemas. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
                return output;
            }

            // Verify and get From reportee party of the delegation
            Party fromParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.From, out string fromOrgNo))
            {
                fromParty = await _registerService.GetOrganisation(fromOrgNo);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.From, out int fromPartyId))
            {
                fromParty = await _contextRetrievalService.GetPartyAsync(fromPartyId);
            }

            if (fromParty == null || fromParty.PartyTypeName != PartyType.Organisation)
            {
                output.Errors.Add("From", $"Maskinporten schema delegation can only be delegated from a valid organization. Invalid value: {delegation.From.FirstOrDefault()?.Id}");
                return output;
            }

            // Verify and get To recipient party of the delegation
            Party toParty = null;
            if (DelegationHelper.TryGetOrganizationNumberFromAttributeMatch(delegation.To, out string toOrgNo))
            {
                toParty = await _registerService.GetOrganisation(toOrgNo);
            }
            else if (DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.To, out int toPartyId))
            {
                toParty = await _contextRetrievalService.GetPartyAsync(toPartyId);
            }
                        
            if (toParty == null || toParty.PartyTypeName != PartyType.Organisation)
            {
                output.Errors.Add("To", $"Maskinporten schema delegation can only be delegated To a valid organization (identified by either {AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute} or {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute} attribute id). Invalid value: {delegation.To.FirstOrDefault()?.Id}");
                return output;
            }

            // Verify authenticated users delegable rights
            RightsQuery rightsQuery = new RightsQuery
            {
                To = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = authenticatedUserId.ToString() } },
                From = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = fromParty.PartyId.ToString() } },
                Resource = right.Resource
            };
            List<Right> usersDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true);
            if (usersDelegableRights == null || usersDelegableRights.Count == 0)
            {
                output.Errors.Add("right[0].Resource", $"Authenticated user does not have any delegable rights for the resource: {resourceRegistryId}");
                return output;
            }

            if (usersDelegableRights.Any(r => r.RightSources.Any(rs => rs.MinimumAuthenticationLevel > authenticatedUserAuthlevel))) 
            {
                output.Errors.Add("right[0].Resource", $"Authenticated user does not meet the required security level requirement for resource: {resourceRegistryId}");
                return output;
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

            List<Rule> result = await _pap.TryWriteDelegationPolicyRules(rulesToDelegate);
            if (result.All(r => r.CreatedSuccessfully))
            {
                output.Rights = usersDelegableRights;
                return await Task.FromResult(output);
            }
            else if (result.Any(r => r.CreatedSuccessfully))
            {
                // Partial delegation of rules should not really be possible. Return success but log error?
                _logger.LogError("One or more rules could not be delegated.\n{result}", result);
                output.Rights = usersDelegableRights;
                return await Task.FromResult(output);
            }

            output.Errors.Add("Rights", "Delegation was not able complete");
            return output;
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

        private async Task<List<Delegation>> GetOfferedDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegationChanges = await _delegationRepository.GetOfferedResourceRegistryDelegations(offeredByPartyId, resourceTypes: resourceType.SingleToList());
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
            List<Delegation> delegations = new List<Delegation>();

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
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId.SingleToList(), resourceTypes: resourceType.SingleToList());
            List<int> parties = new List<int>();
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();

            List<ServiceResource> resources = new List<ServiceResource>();
            List<Tuple<string, string>> resourceIds;
            resourceIds = delegationChanges.Select(d => Tuple.Create(d.ResourceId, d.ResourceType)).ToList();
            resources = await _resourceAdministrationPoint.GetResources(resourceIds);

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            List<Delegation> delegations = new List<Delegation>();

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
            List<ServiceResource> resources;
            List<string> resourceIds;
            
            resources = await _resourceAdministrationPoint.GetResources(scopes);
            resourceIds = resources.Select(d => d.Identifier).ToList();

            List<DelegationChange> delegationChanges = await _delegationRepository.GetResourceRegistryDelegationChanges(resourceIds, supplierPartyId, consumerPartyId, ResourceType.MaskinportenSchema);
            List<int> parties;
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();
            parties.AddRange(delegationChanges.Select(d => d.CoveredByPartyId).Select(ds => Convert.ToInt32(ds)).ToList());

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            List<Delegation> delegations = new List<Delegation>();

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
