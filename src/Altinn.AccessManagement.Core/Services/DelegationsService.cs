using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Utilities;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<DelegationOutput> MaskinportenDelegation(int delegatingUserId, string from, DelegationInput delegation)
        {
            // Verify delegation for single resource registry id
            if (delegation.Rights?.Count != 1)
            {
                throw new ValidationException($"Maskinporten schema delegation only support delegation of a single right identifying the Maskinporten schema resource, registered in the Resource Registry");
            }

            Right right = delegation.Rights.FirstOrDefault();
            if (right == null || !DelegationHelper.TryGetResourceFromAttributeMatch(right.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string _, out string _))
            {
                throw new ValidationException($"The Right is missing a valid Resource specification");
            }

            if (resourceMatchType != ResourceAttributeMatchType.ResourceRegistry)
            {
                throw new ValidationException($"Maskinporten schema delegation only support delegation of resources from the Resource Registry using the {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} attribute id");
            }

            // Verify resource registry id is a valid MaskinportenSchema
            ServiceResource resource = await _contextRetrievalService.GetResource(resourceRegistryId);
            if (resource == null || (resource.IsComplete.HasValue && !resource.IsComplete.Value))
            {
                throw new ValidationException($"The resource: {resourceRegistryId}, does not exist or is not complete and available for delegation");
            }

            if (resource.ResourceType != ResourceType.MaskinportenSchema)
            {
                throw new ValidationException($"Maskinporten schema delegation can only be used to delegate maskinporten schemas. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
            }

            // Verify To recipient of delegation
            if (!DelegationHelper.TryGetPartyIdFromAttributeMatch(delegation.To, out int partyId))
            {
                throw new ValidationException($"Maskinporten schema delegation currently only support delegation To a PartyId (using {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute} attribute id). Invalid value: {delegation.To.FirstOrDefault()?.Id}");
            }

            Party toParty = await _contextRetrievalService.GetPartyAsync(partyId);
            if (toParty == null || toParty.PartyTypeName != PartyType.Organisation)
            {
                throw new ValidationException($"Maskinporten schema delegation can only be delegated To a valid PartyId belonging to an organization. Invalid value: {partyId}");
            }

            // Verify authenticated users delegable rights
            RightsQuery rightsQuery = new RightsQuery
            {
                To = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = delegatingUserId.ToString() } },
                From = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = from } },
                Resource = right.Resource
            };
            List<Right> usersDelegableRights = await _pip.GetRights(rightsQuery, getDelegableRights: true);
            if (usersDelegableRights.IsNullOrEmpty())
            {
                throw new ValidationException($"Authenticated user does not have any delegable rights for the resource: {resourceRegistryId}");
            }

            if (usersDelegableRights.Any(r => !r.CanDelegate.Value)) 
            {
                // ToDo: include SecurityLevelInfo on delegable rights
                throw new ValidationException($"Authenticated user does not meet the required security level requirement for resource: {resourceRegistryId}. Current: N/A Required: N/A");
            }

            // Perform delegation
            List<Rule> rulesToDelegate = new List<Rule>();
            foreach (Right rightToDelegate in usersDelegableRights)
            {
                rulesToDelegate.Add(new Rule
                {
                    DelegatedByUserId = delegatingUserId,
                    OfferedByPartyId = int.Parse(from),
                    CoveredBy = delegation.To,
                    Resource = rightToDelegate.Resource,
                    Action = rightToDelegate.Action
                });
            }

            List<Rule> result = await _pap.TryWriteDelegationPolicyRules(rulesToDelegate);
            if (result.All(r => r.CreatedSuccessfully))
            {
                DelegationOutput output = GetOutputModel(delegation);
                output.Rights = usersDelegableRights;
                return await Task.FromResult(output);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllOutboundDelegationsAsync(string party, ResourceType resourceType)
        {
            int offeredByPartyId = await GetParty(party);
            if (offeredByPartyId == 0)
            {
                throw new ArgumentException("OfferedByPartyId does not have a valid value");
            }

            return await GetOutboundDelegations(offeredByPartyId, resourceType);
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllInboundDelegationsAsync(string party, ResourceType resourceType)
        {
            int coveredByPartyId = await GetParty(party);
            if (coveredByPartyId == 0)
            {
                throw new ArgumentException();
            }

            return await GetInboundDelegations(coveredByPartyId, resourceType);
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
        
        private async Task<List<Delegation>> GetOutboundDelegations(int offeredByPartyId, ResourceType resourceType)
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
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetInboundDelegations(int coveredByPartyId, ResourceType resourceType)
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

        /// <summary>
        /// Gets the party identified by <paramref name="who"/>.
        /// </summary>
        /// <param name="who">
        /// Who, valid values are , an organization number, or a party ID (the letter r followed by 
        /// the party ID).
        /// </param>
        /// <returns>The party identified by <paramref name="who"/>.</returns>
        private async Task<int> GetParty(string who)
        {
            if (string.IsNullOrEmpty(who))
            {
                throw new ArgumentNullException("the parameter who does not have a value");
            }

            try
            {
                int? partyId = DelegationHelper.TryParsePartyId(who);
                if (partyId.HasValue)
                {
                    Party party = await _contextRetrievalService.GetPartyAsync(partyId.Value);
                    partyId = party != null ? party.PartyId : 0;
                    return Convert.ToInt32(partyId);
                }

                return await _contextRetrievalService.GetPartyId(who);
            }   
            catch (Exception ex)
            {
                _logger.LogError("//DelegationsService //GetParty failed to fetch partyid", ex);
                throw;
            }            
        }

        private DelegationOutput GetOutputModel(DelegationInput delegation)
        {
            DelegationOutput result = new DelegationOutput
            {
                To = delegation.To,
                Rights = new()
            };

            foreach (Right right in delegation.Rights)
            {
                result.Rights.Add(right);
            }

            return result;
        }
    }
}
