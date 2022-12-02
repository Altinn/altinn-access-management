using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly CacheConfig _cacheConfig;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<IDelegationsService> _logger;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="cacheConfig">Cache config</param>
        /// <param name="memoryCache">The cache handler </param>
        /// <param name="logger">handler for logger</param>
        /// <param name="delegationRepository">delgation change handler</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        public DelegationsService(IOptions<CacheConfig> cacheConfig, IMemoryCache memoryCache, ILogger<IDelegationsService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService)
        {
            _logger = logger;
            _cacheConfig = cacheConfig.Value;
            _memoryCache = memoryCache;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
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

        private async Task<List<Delegation>> GetOutboundDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegationChanges = await _delegationRepository.GetOfferedResourceRegistryDelegations(offeredByPartyId.SingleToList(), resourceTypes: resourceType.SingleToList());
            List<int> parties = new List<int>();
            foreach (int party in delegationChanges.Select(d => d.CoveredByPartyId).Where(c => c != null).OfType<int>())
            {
                parties.Add(party);
            }

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegationChanges.Select(d => d.ResourceId).Distinct().ToList();

            resources = await _contextRetrievalService.GetResources(resourceIds);

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
                delegation.DelegationResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetInboundDelegations(int coveredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId.SingleToList(), new List<int>(), resourceTypes: resourceType.SingleToList()); // ToDo Fix Optional OfferedBy
            List<int> parties = new List<int>();
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegationChanges.Select(d => d.ResourceId).ToList();
            resources = await _contextRetrievalService.GetResources(resourceIds);

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
                delegation.DelegationResourceType = resource.ResourceType;
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
                    Party party = GetPartyById(partyId.Value);
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

        /// <summary>
        /// Gets a party by party ID.
        /// </summary>
        /// <param name="partyId">Identifies a party (organization or person).</param>
        /// <returns>The identified party.</returns>
        private Party GetPartyById(int partyId)
        {
            Party party;
            party = _contextRetrievalService.GetPartyAsync(partyId).Result;
            return party;
        }
    }
}
