using System.Data;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Persistence;

/// <summary>
/// Repository implementation for PostgreSQL operations on delegations.
/// </summary>
[ExcludeFromCodeCoverage]
public class DelegationMetadataRepository : IDelegationMetadataRepository
{
    private readonly NpgsqlDataSource _conn;
    private readonly ILogger _logger;

    // App DelegationChange functions:
    private readonly string insertAppDelegationChange = "select * from delegation.insert_delegationchange(@_delegationChangeType, @_altinnAppId, @_offeredByPartyId, @_coveredByUserId, @_coveredByPartyId, @_performedByUserId, @_blobStoragePolicyPath, @_blobStorageVersionId)";
    private readonly string getCurrentAppDelegationChange = "select * from delegation.get_current_change(@_altinnAppId, @_offeredByPartyId, @_coveredByUserId, @_coveredByPartyId)";
    private readonly string getAllAppDelegationChanges = "select * from delegation.get_all_changes(@_altinnAppId, @_offeredByPartyId, @_coveredByUserId, @_coveredByPartyId)";
    private readonly string getAppDelegationChangesForCoveredByPartyIds = "select * from delegation.get_all_current_changes_coveredbypartyids(@_altinnAppIds, @_offeredByPartyIds, @_coveredByPartyIds)";
    private readonly string getAppDelegationChangesForCoveredByUserIds = "select * from delegation.get_all_current_changes_coveredbyuserids(@_altinnAppIds, @_offeredByPartyIds, @_coveredByUserIds)";
    private readonly string getAppDelegationChangesOfferedByPartyIds = "select * from delegation.get_all_current_changes_offeredbypartyid_only(@_altinnAppIds, @_offeredByPartyIds)";

    // Resource Registry DelegationChange functions:
    private readonly string insertResourceRegistryDelegationChange = "select * from delegation.insert_resourceregistrydelegationchange(@_delegationChangeType, @_resourceregistryid, @_offeredByPartyId, @_coveredByUserId, @_coveredByPartyId, @_performedByUserId, @_performedbypartyid, @_blobStoragePolicyPath, @_blobStorageVersionId, @_delegatedTime)";
    private readonly string getCurrentResourceRegistryDelegationChange = "select * from delegation.select_current_resourceregistrydelegationchange(@_resourceRegistryId, @_offeredByPartyId, @_coveredByUserId, @_coveredByPartyId)";
    private readonly string getResourceRegistryDelegationChangesForCoveredByPartyIds = "select * from delegation.select_active_resourceregistrydelegationchanges_coveredbypartys(@_coveredByPartyIds, @_offeredByPartyIds, @_resourceRegistryIds, @_resourceTypes)";
    private readonly string getResourceRegistryDelegationChanges = "select * from delegation.select_active_resourceregistrydelegationchanges(@_coveredByPartyIds, @_offeredByPartyIds, @_resourceRegistryIds, @_resourceTypes)";
    private readonly string getResourceRegistryDelegationChangesForCoveredByUserId = "select * from delegation.select_active_resourceregistrydelegationchanges_coveredbyuser(@_coveredByUserId, @_offeredByPartyIds, @_resourceRegistryIds, @_resourceTypes)";
    private readonly string getResourceRegistryDelegationChangesOfferedByPartyId = "select * from delegation.select_active_resourceregistrydelegationchanges_offeredby(@_offeredByPartyId, @_resourceRegistryIds, @_resourceTypes)";

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegationMetadataRepository"/> class
    /// </summary>
    /// <param name="logger">logger</param>
    /// <param name="conn">The database connection</param>
    public DelegationMetadataRepository(ILogger<DelegationMetadataRepository> logger, NpgsqlDataSource conn)
    {
        _logger = logger;
        _conn = conn;
    }

    /// <inheritdoc/>
    public async Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange)
    {
        if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
        {
            return await InsertAppDelegation(delegationChange);
        }

        return await InsertResourceRegistryDelegation(delegationChange);
    }

    /// <inheritdoc/>
    public async Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
    {
        if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
        {
            return await GetCurrentAppDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId);
        }

        return await GetCurrentResourceRegistryDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId);
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getAllAppDelegationChanges, conn);
            pgcom.Parameters.AddWithValue("_altinnAppId", altinnAppId);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
            pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                delegationChanges.Add(await GetAppDelegationChange(reader));
            }

            return delegationChanges;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetAllAppDelegationChanges // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> delegationChanges = new List<DelegationChange>();
        CheckIfOfferedbyPartyIdsHasValue(offeredByPartyIds);

        if (coveredByPartyIds == null && coveredByUserIds == null)
        {
            delegationChanges.AddRange(await GetAllCurrentAppDelegationChangesOfferedByPartyIdOnly(altinnAppIds, offeredByPartyIds));
        }
        else
        {
            if (coveredByPartyIds?.Count > 0)
            {
                delegationChanges.AddRange(await GetAllCurrentAppDelegationChangesCoveredByPartyIds(altinnAppIds, offeredByPartyIds, coveredByPartyIds));
            }

            if (coveredByUserIds?.Count > 0)
            {
                delegationChanges.AddRange(await GetAllCurrentAppDelegationChangesCoveredByUserIds(altinnAppIds, offeredByPartyIds, coveredByUserIds));
            }
        }

        return delegationChanges;
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null)
    {
        List<DelegationChange> delegationChanges = new List<DelegationChange>();
        CheckIfOfferedbyPartyIdsHasValue(offeredByPartyIds);

        if (coveredByPartyIds?.Count > 0)
        {
            delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyIds, offeredByPartyIds, resourceRegistryIds));
        }

        if (coveredByUserId.HasValue)
        {
            delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByUser(coveredByUserId.Value, offeredByPartyIds, resourceRegistryIds));
        }

        return delegationChanges;
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync(cancellationToken);

            NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesOfferedByPartyId, conn);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
            pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceRegistryIds == null || !resourceRegistryIds.Any()) ? DBNull.Value : resourceRegistryIds);
            pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceTypes == null || !resourceTypes.Any()) ? DBNull.Value : resourceTypes.Select(rt => rt.ToString().ToLower()).ToList());

            List<DelegationChange> delegatedResources = new List<DelegationChange>();
            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);
            while (reader.Read())
            {
                delegatedResources.Add(await GetResourceRegistryDelegationChange(reader));
            }

            return delegatedResources;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "AccessManagement // DelegationMetadataRepository // GetOfferedResourceRegistryDelegations // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();
            NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesForCoveredByPartyIds, conn);
            pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, (offeredByPartyIds == null || !offeredByPartyIds.Any()) ? DBNull.Value : offeredByPartyIds);
            pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceRegistryIds == null || !resourceRegistryIds.Any()) ? DBNull.Value : resourceRegistryIds);
            pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceTypes == null || !resourceTypes.Any()) ? DBNull.Value : resourceTypes.Select(rt => rt.ToString().ToLower()).ToList());

            List<DelegationChange> receivedDelegations = new List<DelegationChange>();
            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                receivedDelegations.Add(await GetResourceRegistryDelegationChange(reader));
            }

            return receivedDelegations;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetReceivedResourceRegistryDelegationsForCoveredByParty // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesForCoveredByUserId, conn);
            pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId);
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, (offeredByPartyIds == null || !offeredByPartyIds.Any()) ? DBNull.Value : offeredByPartyIds);
            pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceRegistryIds == null || !resourceRegistryIds.Any()) ? DBNull.Value : resourceRegistryIds);
            pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, (resourceTypes == null || !resourceTypes.Any()) ? DBNull.Value : resourceTypes.Select(rt => rt.ToString().ToLower()).ToList());

            List<DelegationChange> receivedDelegations = new List<DelegationChange>();
            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                receivedDelegations.Add(await GetResourceRegistryDelegationChange(reader));
            }

            return receivedDelegations;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetReceivedResourceRegistryDelegationsForCoveredByParty // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetReceivedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
    {
        const string QUERY = /*strpsql*/@"
            WITH resources AS (
		        SELECT
			        resourceId,
			        resourceRegistryId,
			        resourceType
		        FROM accessmanagement.Resource
		        WHERE resourceType != 'maskinportenschema'
	        ),
            latestResourceChanges AS (
		        SELECT MAX(resourceRegistryDelegationChangeId) as latestId
		        FROM delegation.ResourceRegistryDelegationChanges
		        WHERE offeredbypartyid = ANY (@offeredByPartyIds)
		        GROUP BY resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId
	        ),
	        latestAppChanges AS (
		        SELECT MAX(delegationChangeId) as latestId
		        FROM delegation.delegationchanges
		        WHERE offeredbypartyid = ANY (@offeredByPartyIds)
		        GROUP BY altinnAppId, offeredByPartyId, coveredByUserId, coveredByPartyId
	        )

            SELECT
		        resourceRegistryDelegationChangeId,
		        null AS delegationChangeId,
		        delegationChangeType,
		        resources.resourceRegistryId,
		        resources.resourceType,
		        null AS altinnAppId,
		        offeredByPartyId,
		        coveredByUserId,
		        coveredByPartyId,
		        performedByUserId,
		        performedByPartyId,
		        blobStoragePolicyPath,
		        blobStorageVersionId,
		        created
	        FROM delegation.ResourceRegistryDelegationChanges
		        INNER JOIN resources ON resourceId_fk = resources.resourceid
		        INNER JOIN latestResourceChanges ON resourceRegistryDelegationChangeId = latestResourceChanges.latestId
	        WHERE delegationchangetype != 'revoke_last'

	        UNION ALL

	        SELECT
		        null AS resourceRegistryDelegationChangeId,
		        delegationChangeId,
		        delegationChangeType,
		        null AS resourceRegistryId,
		        null AS resourceType,
		        altinnAppId,
		        offeredByPartyId,
		        coveredByUserId,
		        coveredByPartyId,
		        performedByUserId,
		        null AS performedByPartyId,
		        blobStoragePolicyPath,
		        blobStorageVersionId,
		        created
	        FROM delegation.delegationchanges
		        INNER JOIN latestAppChanges ON delegationchangeid = latestAppChanges.latestId
	        WHERE delegationchangetype != 'revoke_last'
        ";

        try
        {
            await using var pgcom = _conn.CreateCommand(QUERY);
            pgcom.Parameters.AddWithNullableValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);

            return await pgcom.ExecuteEnumerableAsync(cancellationToken)
                .SelectAwait(GetDelegationChange)
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetReceivedDelegations // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
    {
        if (coveredByUserIds == null && coveredByPartyIds == null)
        {
            return await Task.FromResult(new List<DelegationChange>());
        }

        const string QUERY = /*strpsql*/@"
            WITH resources AS (
		        SELECT
			        resourceId,
			        resourceRegistryId,
			        resourceType
		        FROM accessmanagement.Resource
		        WHERE resourceType != 'maskinportenschema'
	        ),
	        latestResourceChanges AS (
		        SELECT MAX(resourceRegistryDelegationChangeId) as latestId
		        FROM delegation.ResourceRegistryDelegationChanges
		        WHERE 
			        coveredByUserId = ANY (@coveredbyuserids)
			        OR coveredByPartyId = ANY (@coveredByPartyIds)
		        GROUP BY resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId
	        ),
	        latestAppChanges AS (
		        SELECT MAX(delegationChangeId) as latestId
		        FROM delegation.delegationchanges
		        WHERE
			        coveredByUserId = ANY (@coveredByUserIds)
			        OR coveredByPartyId = ANY (@coveredByPartyIds)
		        GROUP BY altinnAppId, offeredByPartyId, coveredByUserId, coveredByPartyId
	        )

	        SELECT
		        resourceRegistryDelegationChangeId,
		        null AS delegationChangeId,
		        delegationChangeType,
		        resources.resourceRegistryId,
		        resources.resourceType,
		        null AS altinnAppId,
		        offeredByPartyId,
		        coveredByUserId,
		        coveredByPartyId,
		        performedByUserId,
		        performedByPartyId,
		        blobStoragePolicyPath,
		        blobStorageVersionId,
		        created
	        FROM delegation.ResourceRegistryDelegationChanges
		        INNER JOIN resources ON resourceId_fk = resources.resourceid
		        INNER JOIN latestResourceChanges ON resourceRegistryDelegationChangeId = latestResourceChanges.latestId
	        WHERE delegationchangetype != 'revoke_last'

	        UNION ALL

	        SELECT
		        null AS resourceRegistryDelegationChangeId,
		        delegationChangeId,
		        delegationChangeType,
		        null AS resourceRegistryId,
		        null AS resourceType,
		        altinnAppId,
		        offeredByPartyId,
		        coveredByUserId,
		        coveredByPartyId,
		        performedByUserId,
		        null AS performedByPartyId,
		        blobStoragePolicyPath,
		        blobStorageVersionId,
		        created
	        FROM delegation.delegationchanges
		        INNER JOIN latestAppChanges ON delegationchangeid = latestAppChanges.latestId
	        WHERE delegationchangetype != 'revoke_last'
            ";

        try
        {
            await using var pgcom = _conn.CreateCommand(QUERY);
            pgcom.Parameters.AddWithNullableValue("coveredByUserIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByUserIds);
            pgcom.Parameters.AddWithNullableValue("coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);

            return await pgcom.ExecuteEnumerableAsync(cancellationToken)
                .SelectAwait(GetDelegationChange)
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetAllDelegationChangesForAuthorizedParties // Exception");
            throw;
        }
    }

    private async Task<DelegationChange> InsertAppDelegation(DelegationChange delegationChange)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(insertAppDelegationChange, conn);
            pgcom.Parameters.AddWithValue("_delegationChangeType", delegationChange.DelegationChangeType);
            pgcom.Parameters.AddWithValue("_altinnAppId", delegationChange.ResourceId);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", delegationChange.OfferedByPartyId);
            pgcom.Parameters.AddWithValue("_coveredByUserId", delegationChange.CoveredByUserId.HasValue ? delegationChange.CoveredByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_coveredByPartyId", delegationChange.CoveredByPartyId.HasValue ? delegationChange.CoveredByPartyId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_performedByUserId", delegationChange.PerformedByUserId.HasValue ? delegationChange.PerformedByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_blobStoragePolicyPath", delegationChange.BlobStoragePolicyPath);
            pgcom.Parameters.AddWithValue("_blobStorageVersionId", delegationChange.BlobStorageVersionId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            if (reader.Read())
            {
                return await GetAppDelegationChange(reader);
            }

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // InsertAppDelegation // Exception");
            throw;
        }
    }

    private async Task<DelegationChange> InsertResourceRegistryDelegation(DelegationChange delegationChange)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(insertResourceRegistryDelegationChange, conn);
            pgcom.Parameters.AddWithValue("_delegationChangeType", delegationChange.DelegationChangeType);
            pgcom.Parameters.AddWithValue("_resourceRegistryId", delegationChange.ResourceId);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", delegationChange.OfferedByPartyId);
            pgcom.Parameters.AddWithValue("_coveredByUserId", delegationChange.CoveredByUserId.HasValue ? delegationChange.CoveredByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_coveredByPartyId", delegationChange.CoveredByPartyId.HasValue ? delegationChange.CoveredByPartyId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_performedByUserId", delegationChange.PerformedByUserId.HasValue ? delegationChange.PerformedByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_performedByPartyId", delegationChange.PerformedByPartyId.HasValue ? delegationChange.PerformedByPartyId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_blobStoragePolicyPath", delegationChange.BlobStoragePolicyPath);
            pgcom.Parameters.AddWithValue("_blobStorageVersionId", delegationChange.BlobStorageVersionId);
            pgcom.Parameters.AddWithValue("_delegatedTime", delegationChange.Created.HasValue ? delegationChange.Created.Value : DateTime.UtcNow);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            if (reader.Read())
            {
                return await GetResourceRegistryDelegationChange(reader);
            }

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // InsertResourceRegistryDelegation // Exception");
            throw;
        }
    }

    private async Task<DelegationChange> GetCurrentAppDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getCurrentAppDelegationChange, conn);

            pgcom.Parameters.AddWithValue("_altinnAppId", resourceId);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
            pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            if (reader.Read())
            {
                return await GetAppDelegationChange(reader);
            }

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetCurrentAppDelegation // Exception");
            throw;
        }
    }

    private async Task<DelegationChange> GetCurrentResourceRegistryDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getCurrentResourceRegistryDelegationChange, conn);

            pgcom.Parameters.AddWithValue("_resourceRegistryId", resourceId);
            pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
            pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
            pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            if (reader.Read())
            {
                return await GetResourceRegistryDelegationChange(reader);
            }

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetCurrentResourceRegistryDelegation // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChanges, conn);
            pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyId == 0 ? DBNull.Value : new List<int> { coveredByPartyId });
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyId == 0 ? DBNull.Value : new List<int> { offeredByPartyId });
            pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceIds);
            pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, new List<string> { resourceType.ToString().ToLower() });

            List<DelegationChange> receivedDelegations = new List<DelegationChange>();
            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                receivedDelegations.Add(await GetResourceRegistryDelegationChange(reader));
            }

            return receivedDelegations;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetCurrentDelegationChange // Exception");
            throw;
        }
    }

    private static void CheckIfOfferedbyPartyIdsHasValue(List<int> offeredByPartyIds)
    {
        if (offeredByPartyIds == null)
        {
            throw new ArgumentNullException(nameof(offeredByPartyIds));
        }
        else if (offeredByPartyIds.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offeredByPartyIds));
        }
    }

    private static async ValueTask<DelegationChange> GetDelegationChange(NpgsqlDataReader reader)
    {
        return await reader.GetFieldValueAsync<int?>("resourceregistrydelegationchangeid") > 0
            ? await GetResourceRegistryDelegationChange(reader)
            : await GetAppDelegationChange(reader);
    }

    private static async ValueTask<DelegationChange> GetAppDelegationChange(NpgsqlDataReader reader)
    {
        try
        {
            return new DelegationChange
            {
                DelegationChangeId = await reader.GetFieldValueAsync<int>("delegationchangeid"),
                DelegationChangeType = await reader.GetFieldValueAsync<DelegationChangeType>("delegationchangetype"),
                ResourceId = await reader.GetFieldValueAsync<string>("altinnappid"),
                ResourceType = ResourceAttributeMatchType.AltinnAppId.ToString(),
                OfferedByPartyId = await reader.GetFieldValueAsync<int>("offeredbypartyid"),
                CoveredByPartyId = await reader.GetFieldValueAsync<int?>("coveredbypartyid"),
                CoveredByUserId = await reader.GetFieldValueAsync<int?>("coveredbyuserid"),
                PerformedByUserId = await reader.GetFieldValueAsync<int?>("performedbyuserid"),
                BlobStoragePolicyPath = await reader.GetFieldValueAsync<string>("blobstoragepolicypath"),
                BlobStorageVersionId = await reader.GetFieldValueAsync<string>("blobstorageversionid"),
                Created = await reader.GetFieldValueAsync<DateTime>("created")
            };
        }
        catch (Exception e)
        {
            return await new ValueTask<DelegationChange>(Task.FromException<DelegationChange>(e));
        }
    }

    private static async ValueTask<DelegationChange> GetResourceRegistryDelegationChange(NpgsqlDataReader reader)
    {
        try
        {
            return new DelegationChange
            {
                ResourceRegistryDelegationChangeId = await reader.GetFieldValueAsync<int>("resourceregistrydelegationchangeid"),
                DelegationChangeType = await reader.GetFieldValueAsync<DelegationChangeType>("delegationchangetype"),
                ResourceId = await reader.GetFieldValueAsync<string>("resourceregistryid"),
                ResourceType = await reader.GetFieldValueAsync<string>("resourcetype"),
                OfferedByPartyId = await reader.GetFieldValueAsync<int>("offeredbypartyid"),
                CoveredByPartyId = await reader.GetFieldValueAsync<int?>("coveredbypartyid"),
                CoveredByUserId = await reader.GetFieldValueAsync<int?>("coveredbyuserid"),
                PerformedByUserId = await reader.GetFieldValueAsync<int?>("performedbyuserid"),
                PerformedByPartyId = await reader.GetFieldValueAsync<int?>("performedbypartyid"),
                BlobStoragePolicyPath = await reader.GetFieldValueAsync<string>("blobstoragepolicypath"),
                BlobStorageVersionId = await reader.GetFieldValueAsync<string>("blobstorageversionid"),
                Created = await reader.GetFieldValueAsync<DateTime>("created")
            };
        }
        catch (Exception e)
        {
            return await new ValueTask<DelegationChange>(Task.FromException<DelegationChange>(e));
        }
    }

    private async Task<List<DelegationChange>> GetAllCurrentAppDelegationChangesCoveredByPartyIds(List<string> altinnAppIds = null, List<int> offeredByPartyIds = null, List<int> coveredByPartyIds = null)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesForCoveredByPartyIds, conn);
            pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlDbType.Array | NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);
            pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                delegationChanges.Add(await GetAppDelegationChange(reader));
            }

            return delegationChanges;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetAllCurrentAppDelegationChangesCoveredByPartyIds // Exception");
            throw;
        }
    }

    private async Task<List<DelegationChange>> GetAllCurrentAppDelegationChangesCoveredByUserIds(List<string> altinnAppIds = null, List<int> offeredByPartyIds = null, List<int> coveredByUserIds = null)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesForCoveredByUserIds, conn);
            pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlDbType.Array | NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);
            pgcom.Parameters.AddWithValue("_coveredByUserIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByUserIds);

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                delegationChanges.Add(await GetAppDelegationChange(reader));
            }

            return delegationChanges;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetAllCurrentAppDelegationChangesCoveredByUserIds // Exception");
            throw;
        }
    }

    private async Task<List<DelegationChange>> GetAllCurrentAppDelegationChangesOfferedByPartyIdOnly(List<string> altinnAppIds = null, List<int> offeredByPartyIds = null)
    {
        try
        {
            var conn = await _conn.OpenConnectionAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesOfferedByPartyIds, conn);
            pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlDbType.Array | NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
            pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

            using NpgsqlDataReader reader = pgcom.ExecuteReader();
            while (reader.Read())
            {
                delegationChanges.Add(await GetAppDelegationChange(reader));
            }

            return delegationChanges;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetAllCurrentAppDelegationChangesOfferedByPartyIdOnly // Exception");
            throw;
        }
    }
}
