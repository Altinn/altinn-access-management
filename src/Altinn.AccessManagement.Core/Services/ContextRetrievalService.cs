using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services;

/// <summary>
/// Context Retrieval Service
/// </summary>
public class ContextRetrievalService : IContextRetrievalService
{
    private readonly CacheConfig _cacheConfig;
    private readonly IMemoryCache _memoryCache;
    private readonly IResourceRegistryClient _resourceRegistryClient;
    private readonly IAltinnRolesClient _altinnRolesClient;
    private readonly IPartiesClient _partiesClient;
    private readonly IAuthenticationClient _authenticationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextRetrievalService"/> class
    /// </summary>
    /// <param name="cacheConfig">Cache config</param>
    /// <param name="memoryCache">The cache handler </param>
    /// <param name="resourceRegistryClient">The client for integration with the ResourceRegistry</param>
    /// <param name="altinnRolesClient">The client for integration with the SBL Bridge for role information</param>
    /// <param name="partiesClient">The client for integration </param>
    /// <param name="authenticationClient">The client for integration with authentication</param>
    public ContextRetrievalService(IOptions<CacheConfig> cacheConfig, IMemoryCache memoryCache, IResourceRegistryClient resourceRegistryClient, IAltinnRolesClient altinnRolesClient, IPartiesClient partiesClient, IAuthenticationClient authenticationClient)
    {
        _cacheConfig = cacheConfig.Value;
        _memoryCache = memoryCache;
        _resourceRegistryClient = resourceRegistryClient;
        _altinnRolesClient = altinnRolesClient;
        _partiesClient = partiesClient;
        _authenticationClient = authenticationClient;
    }

    /// <inheritdoc/>
    public async Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"Roles_u:{coveredByUserId}_p:{offeredByPartyId}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<Role> roles))
        {
            roles = await _altinnRolesClient.GetDecisionPointRolesForUser(coveredByUserId, offeredByPartyId, cancellationToken) ?? new List<Role>();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetPriority(CacheItemPriority.High)
           .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.AltinnRoleCacheTimeout, 0));

            _memoryCache.Set(cacheKey, roles, cacheEntryOptions);
        }

        return roles;
    }

    /// <inheritdoc/>
    public async Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"DelgRoles_u:{coveredByUserId}_p:{offeredByPartyId}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<Role> roles))
        {
            roles = await _altinnRolesClient.GetRolesForDelegation(coveredByUserId, offeredByPartyId, cancellationToken) ?? new List<Role>();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetPriority(CacheItemPriority.High)
           .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.AltinnRoleCacheTimeout, 0));

            _memoryCache.Set(cacheKey, roles, cacheEntryOptions);
        }

        return roles;
    }

    /// <inheritdoc/>
    public async Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default)
    {
        List<Party> result = await GetPartiesAsync(partyId.SingleToList(), cancellationToken: cancellationToken);
        return result.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        List<Party> parties = new List<Party>();
        List<int> partyIdsNotInCache = new List<int>();

        foreach (int partyId in partyIds.Distinct())
        {
            if (_memoryCache.TryGetValue($"p:{partyId}|inclSubunits:{includeSubunits}", out Party party))
            {
                parties.Add(party);
            }
            else
            {
                partyIdsNotInCache.Add(partyId);
            }
        }

        if (partyIdsNotInCache.Count == 0)
        {
            return parties;
        }

        List<Party> remainingParties = await _partiesClient.GetPartiesAsync(partyIdsNotInCache, includeSubunits, cancellationToken);
        if (remainingParties.Count > 0)
        {
            foreach (Party party in remainingParties)
            {
                if (party?.PartyId != null)
                {
                    parties.Add(party);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetPriority(CacheItemPriority.High)
                   .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));
                    _memoryCache.Set($"p:{party.PartyId}|inclSubunits:{includeSubunits}", party, cacheEntryOptions);
                }
            }
        }

        return parties;
    }

    /// <inheritdoc/>
    public async Task<SortedDictionary<int, Party>> GetPartiesAsSortedDictionaryAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        SortedDictionary<int, Party> dict = [];

        List<Party> parties = await GetPartiesAsync(partyIds, includeSubunits, cancellationToken);
        foreach (Party party in parties)
        {
            dict.Add(party.PartyId, party);
        }

        return dict;
    }

    /// <inheritdoc/>
    public async Task<Party> GetPartyByUuid(Guid partyUuid, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        Dictionary<string, Party> parties = await GetPartiesByUuids(partyUuid.SingleToList(), includeSubunits, cancellationToken);
        parties.TryGetValue(partyUuid.ToString(), out Party result);
        return result;
    }

    /// <inheritdoc /> 
    public async Task<SystemUser> GetSystemUserById(int partyId, string systemUserUuid, CancellationToken cancellationToken = default)
    {
        SystemUser systemUser = await _authenticationClient.GetSystemUser(partyId, systemUserUuid, cancellationToken);
        return systemUser;
    }

    /// <inheritdoc />
    public async Task<List<DefaultRight>> GetDefaultRightsForRegisteredSystem(string productId, CancellationToken cancellationToken = default)
    {
        List<DefaultRight> defaultRight = await _authenticationClient.GetDefaultRightsForRegisteredSystem(productId, cancellationToken);
        return defaultRight;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Party>> GetPartiesByUuids(IEnumerable<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        Dictionary<string, Party> parties = new Dictionary<string, Party>();
        List<Guid> partyKeysNotInCache = new List<Guid>();

        foreach (Guid partyKey in partyUuids.Distinct())
        {
            if (_memoryCache.TryGetValue($"uuid:{partyKey}|inclSubunits:{includeSubunits}", out Party party))
            {
                parties.Add($"{party.PartyUuid}", party);
            }
            else
            {
                partyKeysNotInCache.Add(partyKey);
            }
        }

        if (partyKeysNotInCache.Count == 0)
        {
            return parties;
        }

        List<Party> remainingParties = await _partiesClient.GetPartiesAsync(partyKeysNotInCache, includeSubunits, cancellationToken);
        if (remainingParties.Count > 0)
        {
            foreach (Party party in remainingParties)
            {
                if (party != null && party.PartyUuid != Guid.Empty)
                {
                    parties.Add($"{party.PartyUuid}", party);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetPriority(CacheItemPriority.High)
                   .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));
                    _memoryCache.Set($"uuid:{party.PartyUuid}|inclSubunits:{includeSubunits}", party, cacheEntryOptions);
                }
            }
        }

        return parties;
    }

 /// <inheritdoc/>
    public async Task<Party> GetPartyForOrganization(string organizationNumber, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"orgNo:{organizationNumber}";

        if (!_memoryCache.TryGetValue(cacheKey, out Party party))
        {
            party = await _partiesClient.LookupPartyBySSNOrOrgNo(new PartyLookup { OrgNo = organizationNumber }, cancellationToken);

            if (party != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));

                _memoryCache.Set(cacheKey, party, cacheEntryOptions);
            }
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<Party> GetPartyForPerson(string ssn, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"ssn:{ssn}";

        if (!_memoryCache.TryGetValue(cacheKey, out Party party))
        {
            party = await _partiesClient.LookupPartyBySSNOrOrgNo(new PartyLookup { Ssn = ssn }, cancellationToken);

            if (party != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));

                _memoryCache.Set(cacheKey, party, cacheEntryOptions);
            }
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetKeyRolePartyIds(int userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"KeyRolePartyIds_u:{userId}";
        if (!_memoryCache.TryGetValue(cacheKey, out List<int> keyrolePartyIds))
        {
            keyrolePartyIds = await _partiesClient.GetKeyRoleParties(userId, cancellationToken);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetPriority(CacheItemPriority.High)
           .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.KeyRolePartyIdsCacheTimeout, 0));

            _memoryCache.Set(cacheKey, keyrolePartyIds, cacheEntryOptions);
        }

        return keyrolePartyIds;
    }

    /// <inheritdoc/>
    public async Task<List<MainUnit>> GetMainUnits(List<int> subunitPartyIds, CancellationToken cancellationToken = default)
    {
        List<MainUnit> allMainUnits = new List<MainUnit>();
        List<int> subunitsNotInCache = new List<int>();

        foreach (int subunitPartyId in subunitPartyIds.Distinct())
        {
            if (_memoryCache.TryGetValue($"subunit:{subunitPartyId}", out MainUnit mainUnit))
            {
                allMainUnits.Add(mainUnit);
            }
            else
            {
                subunitsNotInCache.Add(subunitPartyId);
            }
        }

        if (subunitsNotInCache.Count == 0)
        {
            return allMainUnits;
        }

        List<MainUnit> remainingMainUnits = await _partiesClient.GetMainUnits(new MainUnitQuery { PartyIds = subunitsNotInCache }, cancellationToken);
        if (remainingMainUnits.Count > 0)
        {
            foreach (MainUnit mainUnit in remainingMainUnits)
            {
                allMainUnits.Add(mainUnit);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High)
                    .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.MainUnitCacheTimeout, 0));
                _memoryCache.Set($"subunit:{mainUnit.SubunitPartyId}", mainUnit, cacheEntryOptions);
            }
        }

        return allMainUnits;
    }

    /// <inheritdoc/>
    public async Task<MainUnit> GetMainUnit(int subunitPartyId, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue($"subunit:{subunitPartyId}", out MainUnit mainUnit))
        {
            return mainUnit;
        }

        List<MainUnit> mainUnits = await GetMainUnits(subunitPartyId.SingleToList(), cancellationToken);
        return mainUnits.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<ServiceResource> GetResource(string resourceRegistryId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"rrId:{resourceRegistryId}";

        if (!_memoryCache.TryGetValue(cacheKey, out ServiceResource resource))
        {
            resource = await _resourceRegistryClient.GetResource(resourceRegistryId, cancellationToken);

            if (resource != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetPriority(CacheItemPriority.High)
                   .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resource, cacheEntryOptions);
            }
        }

        return resource;
    }

    /// <inheritdoc/>
    public async Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default)
    {
        string cacheKey = $"resources:all";

        if (!_memoryCache.TryGetValue(cacheKey, out List<ServiceResource> resources))
        {
            resources = await _resourceRegistryClient.GetResources(cancellationToken);

            if (resources?.Count > 0)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resources, cacheEntryOptions);
            }
        }

        return resources;
    }

    /// <inheritdoc/>
    public async Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default)
    {
        string cacheKey = $"resources:resourceList";

        if (!_memoryCache.TryGetValue(cacheKey, out List<ServiceResource> resources))
        {
            resources = await _resourceRegistryClient.GetResourceList(cancellationToken);

            if (resources?.Count > 0)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resources, cacheEntryOptions);
            }
        }

        return resources;
    }

    /// <inheritdoc/>
    public async Task<ServiceResource> GetResourceFromResourceList(string resourceId = null, string org = null, string app = null, string serviceCode = null, string serviceEditionCode = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"r:{resourceId},o:{org},a:{app},sc:{serviceCode},sec:{serviceEditionCode}";

        if (!_memoryCache.TryGetValue(cacheKey, out ServiceResource resource))
        {
            List<ServiceResource> resources = await GetResourceList(cancellationToken);
            foreach (ServiceResource serviceResource in resources)
            {
                if (resourceId != null && (serviceResource.ResourceType != ResourceType.Altinn2Service || serviceResource.ResourceType != ResourceType.AltinnApp) &&
                    serviceResource.Identifier == resourceId)
                {
                    resource = serviceResource;
                    break;
                }

                if (org != null && app != null && serviceResource.ResourceType == ResourceType.AltinnApp &&
                    serviceResource.ResourceReferences.Exists(rf => rf.ReferenceType == ReferenceType.ApplicationId && string.Equals(rf.Reference, $"{org}/{app}", StringComparison.OrdinalIgnoreCase)))
                {
                    resource = serviceResource;
                    break;
                }

                if (serviceCode != null && serviceEditionCode != null && serviceResource.ResourceType == ResourceType.Altinn2Service &&
                    serviceResource.ResourceReferences.Exists(rf => rf.ReferenceType == ReferenceType.ServiceCode && string.Equals(rf.Reference, $"{serviceCode}", StringComparison.OrdinalIgnoreCase)) &&
                    serviceResource.ResourceReferences.Exists(rf => rf.ReferenceType == ReferenceType.ServiceEditionCode && string.Equals(rf.Reference, $"{serviceEditionCode}", StringComparison.OrdinalIgnoreCase)))
                {
                    resource = serviceResource;
                    break;
                }
            }

            if (resource != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resource, cacheEntryOptions);
            }
        }

        return resource;
    }

    /// <inheritdoc/>
    public async Task<Party> GetPartyForUser(int userId, int partyId, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = await GetPartiesForUser(userId, cancellationToken);

        foreach (Party party in partyList)
        {
            if (party?.PartyId == partyId)
            {
                return party;
            }

            Party childParty = party?.ChildParties?.FirstOrDefault(p => p.PartyId == partyId);
            if (childParty != null)
            {
                return childParty;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default)
    {
        Dictionary<string, IEnumerable<BaseAttribute>> subjectResources = new Dictionary<string, IEnumerable<BaseAttribute>>();
        List<string> subjectKeysNotInCache = new List<string>();

        foreach (string subjectKey in subjects.Distinct())
        {
            if (_memoryCache.TryGetValue(subjectKey, out IEnumerable<BaseAttribute> resources))
            {
                subjectResources.Add(subjectKey, resources);
            }
            else
            {
                subjectKeysNotInCache.Add(subjectKey);
            }
        }

        if (subjectKeysNotInCache.Count == 0)
        {
            return subjectResources;
        }

        IDictionary<string, IEnumerable<BaseAttribute>> remainingSubjectResources = await _resourceRegistryClient.GetSubjectResources(subjectKeysNotInCache, cancellationToken);
        foreach (string subject in subjectKeysNotInCache)
        {
            IEnumerable<BaseAttribute> resources;
            if (remainingSubjectResources.TryGetValue(subject, out resources))
            {
                subjectResources.Add(subject, resources);
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetPriority(CacheItemPriority.High)
                   .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistrySubjectResourcesCacheTimeout, 0));
            _memoryCache.Set(subject, resources ?? Enumerable.Empty<BaseAttribute>(), cacheEntryOptions);
        }

        return subjectResources;
    }

    private async Task<List<Party>> GetPartiesForUser(int userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"userId:{userId}";

        if (_memoryCache.TryGetValue(cacheKey, out List<Party> partyListFromCache))
        {
            return partyListFromCache;
        }

        List<Party> partyList = await _partiesClient.GetPartiesForUserAsync(userId, cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetPriority(CacheItemPriority.High)
          .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));

        _memoryCache.Set(cacheKey, partyList, cacheEntryOptions);

        return partyList;
    }
}
