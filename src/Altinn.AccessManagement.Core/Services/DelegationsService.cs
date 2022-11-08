using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Enums;
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
        public async Task<List<DelegationChange>> FindAllDelegations(int subjectUserId, int reporteePartyId, string resourceRegistryId)
        {
            Dictionary<DelegationType, List<DelegationChange>> result = new Dictionary<DelegationType, List<DelegationChange>>();
            List<int> offeredByPartyIds = new List<int> { reporteePartyId };
            List<int> coveredByUserIds = new List<int> { subjectUserId };

            // 1. Direct user delegations
            List<DelegationChange> delegations = await GetAllCachedDelegationChanges(offeredByPartyIds, resourceRegistryId.SingleToList(), coveredByUserIds: coveredByUserIds);
            ////result.Add(DelegationType.DirectUserDelegation, delegations);

            // 2. Direct user delegations from mainunit
            List<MainUnit> mainunits = await _contextRetrievalService.GetMainUnits(reporteePartyId);
            List<int> mainunitPartyIds = mainunits.Where(m => m.PartyId.HasValue).Select(m => m.PartyId.Value).ToList();

            if (mainunitPartyIds.Any())
            {
                offeredByPartyIds.AddRange(mainunitPartyIds);
                List<DelegationChange> directMainUnitDelegations = await GetAllCachedDelegationChanges(mainunitPartyIds, resourceRegistryId.SingleToList(), coveredByUserIds: coveredByUserIds);

                if (directMainUnitDelegations.Any())
                {
                    delegations.AddRange(directMainUnitDelegations);
                    ////result.Add(DelegationType., directMainUnitDelegations);
                }
            }

            // 3. Direct party delegations to keyrole units
            List<int> keyrolePartyIds = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId);
            if (keyrolePartyIds.Any())
            {
                List<DelegationChange> keyRoleDelegations = await GetAllCachedDelegationChanges(offeredByPartyIds, resourceRegistryId.SingleToList(), coveredByPartyIds: keyrolePartyIds);

                if (keyRoleDelegations.Any())
                {
                    delegations.AddRange(keyRoleDelegations);
                }
            }

            return delegations;
        }

        /// <inheritdoc/>
        public async Task<List<OfferedDelegations>> GetAllOfferedDelegations(int offeredbyPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegations = await _delegationRepository.GetAllOfferedDelegations(offeredbyPartyId, resourceType);
            List<int> parties = new List<int>();
            foreach (int party in delegations.Select(d => d.CoveredByPartyId).Where(c => c != null))
            {
                parties.Add(party);
            }

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegations.Select(d => d.ResourceId).Distinct().ToList();

            resources = await _contextRetrievalService.GetResources(resourceIds);

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            List<OfferedDelegations> resourceDelegations = new List<OfferedDelegations>();
            foreach (ServiceResource resource in resources)
            {
                OfferedDelegations resourceDelegation = new OfferedDelegations();
                resourceDelegation.ResourceId = resource.Identifier;
                resourceDelegation.ResourceTitle = resource.Title.FirstOrDefault().Value;
                List<DelegationChange> query = delegations.FindAll(d => d.ResourceId.Equals(resource.Identifier));
                resourceDelegation.Delegations = new List<Delegation>();

                foreach (DelegationChange delegationChange in query)
                {
                    Delegation delegation = new Delegation();
                    Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                    delegation.CoveredByName = partyInfo?.Name;
                    delegation.CoveredByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                    delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                    delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                    delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                    delegation.Created = delegationChange.Created;
                    resourceDelegation.Delegations.Add(delegation);
                }

                resourceDelegations.Add(resourceDelegation);
            }

            return resourceDelegations;
        }

        /// <inheritdoc/>
        public async Task<List<ReceivedDelegation>> GetReceivedDelegationsAsync(int coveredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegations = await _delegationRepository.GetReceivedDelegationsAsync(coveredByPartyId, resourceType);
            List<int> parties = new List<int>();
            parties = delegations.Select(d => d.OfferedByPartyId).ToList();

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegations.Select(d => d.ResourceId).ToList();
            resources = await _contextRetrievalService.GetResources(resourceIds);

            List<Party> partyList = await _contextRetrievalService.GetPartiesAsync(parties);
            List<ReceivedDelegation> receivedDelegations = new List<ReceivedDelegation>();
            foreach (Party party in partyList)
            {
                if (receivedDelegations.FindAll(rd => rd.OfferedByPartyId.Equals(party.PartyId)).Count <= 0)
                {
                    ReceivedDelegation receivedDelegation = new ReceivedDelegation();
                    receivedDelegation.OfferedByPartyId = party.PartyId;
                    receivedDelegation.OfferedByName = party.Name;
                    receivedDelegation.OfferedByOrgNumber = Convert.ToInt32(party.OrgNumber);
                    List<DelegationChange> query = delegations.FindAll(d => d.CoveredByPartyId.Equals(coveredByPartyId) && d.OfferedByPartyId.Equals(party.PartyId));
                    receivedDelegation.Resources = new List<ServiceResource>();

                    foreach (DelegationChange delegation in query)
                    {
                        ServiceResource resource = resources.Find(d => d.Identifier == delegation.ResourceId);
                        receivedDelegation.Resources.Add(resource);
                    }

                    receivedDelegations.Add(receivedDelegation);
                }
            }

            return receivedDelegations;
        }

        private async Task<List<DelegationChange>> GetAllCachedDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null)
        {
            List<DelegationChange> delegationChanges;
            string cacheKey = GetDlgChangeCacheKey(offeredByPartyIds, resourceRegistryIds, coveredByPartyIds, coveredByUserIds);
            if (!_memoryCache.TryGetValue(cacheKey, out delegationChanges))
            {
                delegationChanges = await _delegationRepository.GetAllCurrentDelegationChanges(offeredByPartyIds, resourceRegistryIds, coveredByPartyIds, coveredByUserIds);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, 0, 5, 0)); // Should GetRight use cache? How long?

                _memoryCache.Set(cacheKey, delegationChanges, cacheEntryOptions);
            }

            return delegationChanges;
        }

        private string GetDlgChangeCacheKey(List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null)
        {
            string cacheKey = null;
            foreach (int id in offeredByPartyIds ?? Enumerable.Empty<int>())
            {
                cacheKey += $"o:{id};";
            }

            foreach (string id in resourceRegistryIds ?? Enumerable.Empty<string>())
            {
                cacheKey += $"a:{id};";
            }

            foreach (int id in coveredByPartyIds ?? Enumerable.Empty<int>())
            {
                cacheKey += $"p:{id};";
            }

            foreach (int id in coveredByUserIds ?? Enumerable.Empty<int>())
            {
                cacheKey += $"u:{id};";
            }

            return cacheKey;
        }
    }
}
