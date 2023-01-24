using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Utilities;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly ILogger<IDelegationsService> _logger;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IResourceAdministrationPoint _resourceAdministrationPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="logger">handler for logger</param>
        /// <param name="delegationRepository">delgation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="resourceAdministrationPoint">handler for resoruce registry</param>
        public DelegationsService(ILogger<IDelegationsService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IResourceAdministrationPoint resourceAdministrationPoint)
        {
            _logger = logger;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _resourceAdministrationPoint = resourceAdministrationPoint;
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> FindAllDelegations(int subjectUserId, int reporteePartyId, string resourceId, ResourceAttributeMatchType resourceMatchType)
        {
            if (resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new NotSupportedException("Must specify the resource match type");
            }

            List<DelegationChange> delegations = new List<DelegationChange>();
            List<int> offeredByPartyIds = reporteePartyId.SingleToList();
            List<string> resourceIds = resourceId.SingleToList();

            // 1. Direct user delegations
            List<DelegationChange> userDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserIds: subjectUserId.SingleToList()) :
                await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserId: subjectUserId);
            delegations.AddRange(userDelegations);

            // 2. Direct user delegations from mainunit
            List<MainUnit> mainunits = await _contextRetrievalService.GetMainUnits(reporteePartyId);
            List<int> mainunitPartyIds = mainunits.Where(m => m.PartyId.HasValue).Select(m => m.PartyId.Value).ToList();

            if (mainunitPartyIds.Any())
            {
                offeredByPartyIds.AddRange(mainunitPartyIds);
                List<DelegationChange> directMainUnitDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                    await _delegationRepository.GetAllCurrentAppDelegationChanges(mainunitPartyIds, resourceIds, coveredByUserIds: subjectUserId.SingleToList()) :
                    await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(mainunitPartyIds, resourceIds, coveredByUserId: subjectUserId);

                if (directMainUnitDelegations.Any())
                {
                    delegations.AddRange(directMainUnitDelegations);
                }
            }

            // 3. Direct party delegations to keyrole units
            List<int> keyrolePartyIds = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId);
            if (keyrolePartyIds.Any())
            {
                List<DelegationChange> keyRoleDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                    await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: keyrolePartyIds) :
                    await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: keyrolePartyIds);

                if (keyRoleDelegations.Any())
                {
                    delegations.AddRange(keyRoleDelegations);
                }
            }

            return delegations;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllOutboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int offeredByPartyId = 0;

            offeredByPartyId = await GetParty(who);
            if (offeredByPartyId == 0)
            {
                throw new ArgumentException("OfferedByPartyId does not have a valid value");
            }

            return await GetOutboundDelegations(offeredByPartyId, resourceType);
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllInboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int coveredByPartyId = 0;

            coveredByPartyId = await GetParty(who);
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

        #region private methods

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
                delegation.HasCompetentAuthority = resource.HasCompetentAuthority;
                delegation.Description = resource.Description;
                delegation.RightDescription = resource.RightDescription;
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
        #endregion
    }
}
