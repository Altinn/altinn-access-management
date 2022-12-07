using System.Data;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Repository implementation for PostgreSQL operations on delegations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DelegationMetadataRepository : IDelegationMetadataRepository
    {
        private readonly string _connectionString;
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
        private readonly string getResourceRegistryDelegationChangesForCoveredByUserId = "select * from delegation.select_active_resourceregistrydelegationchanges_coveredbyuser(@_coveredByUserId, @_offeredByPartyIds, @_resourceRegistryIds, @_resourceTypes)";
        private readonly string getResourceRegistryDelegationChangesOfferedByPartyId = "select * from delegation.select_active_resourceregistrydelegationchanges_offeredby(@_offeredByPartyId, @_resourceRegistryIds, @_resourceTypes)";
        private readonly string searchDelegationsSql = "select * from delegation.select_active_resourceregistrydelegationchanges_admin(@_offeredByPartyId, @_coveredByPartyId, @_resourceIds, @_resourcetypes)";

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationMetadataRepository"/> class
        /// </summary>
        /// <param name="postgresSettings">The postgreSQL configurations for AuthorizationDB</param>
        /// <param name="logger">logger</param>
        public DelegationMetadataRepository(
            IOptions<PostgreSQLSettings> postgresSettings,
            ILogger<DelegationMetadataRepository> logger)
        {
            _logger = logger;
            _connectionString = string.Format(
                postgresSettings.Value.ConnectionString,
                postgresSettings.Value.AuthorizationDbPwd);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DelegationChangeType>("delegation.delegationchangetype");
        }

        /// <inheritdoc/>
        public async Task<DelegationChange> InsertDelegation(DelegationChange delegationChange)
        {
            if (delegationChange.ResourceType == ResourceAttributeMatchType.AltinnAppId.ToString())
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getAllAppDelegationChanges, conn);
                pgcom.Parameters.AddWithValue("_altinnAppId", altinnAppId);
                pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
                pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
                pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

                List<DelegationChange> delegationChanges = new List<DelegationChange>();

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    delegationChanges.Add(GetAppDelegationChange(reader));
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
        public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null)
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
        public async Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesOfferedByPartyId, conn);
                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, new List<int> { offeredByPartyId });
                pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, new List<string> { resourceType.ToString().ToLower() });

                pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                List<DelegationChange> delegatedResources = new List<DelegationChange>();
                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    delegatedResources.Add(GetResourceRegistryDelegationChange(reader));
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
        public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(int coveredByPartyId, ResourceType resourceType)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesForCoveredByPartyIds, conn);
                pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, new List<int> { coveredByPartyId });
                pgcom.Parameters.AddWithValue("_resourceType", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, new List<string> { resourceType.ToString().ToLower() });

                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                List<DelegationChange> receivedDelegations = new List<DelegationChange>();
                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    receivedDelegations.Add(GetResourceRegistryDelegationChange(reader));
                }

                return receivedDelegations;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Authorization // DelegationMetadataRepository // GetReceivedResourceRegistryDelegationsForCoveredByParty // Exception");
                throw;
            }
        }

        private async Task<DelegationChange> InsertAppDelegation(DelegationChange delegationChange)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

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
                    return GetAppDelegationChange(reader);
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

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
                    return GetResourceRegistryDelegationChange(reader);
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                NpgsqlCommand pgcom = new NpgsqlCommand(getCurrentAppDelegationChange, conn);

                pgcom.Parameters.AddWithValue("_altinnAppId", resourceId);
                pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
                pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
                pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                if (reader.Read())
                {
                    return GetAppDelegationChange(reader);
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                NpgsqlCommand pgcom = new NpgsqlCommand(getCurrentResourceRegistryDelegationChange, conn);

                pgcom.Parameters.AddWithValue("_resourceRegistryId", resourceId);
                pgcom.Parameters.AddWithValue("_offeredByPartyId", offeredByPartyId);
                pgcom.Parameters.AddWithValue("_coveredByUserId", coveredByUserId.HasValue ? coveredByUserId.Value : DBNull.Value);
                pgcom.Parameters.AddWithValue("_coveredByPartyId", coveredByPartyId.HasValue ? coveredByPartyId.Value : DBNull.Value);

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                if (reader.Read())
                {
                    return GetResourceRegistryDelegationChange(reader);
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
        public async Task<List<DelegationChange>> GetResourceRegistryDelegationChangesForAdmin(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getResourceRegistryDelegationChangesForCoveredByPartyIds, conn);
                pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, coveredByPartyId == 0 ? DBNull.Value : new List<int> { coveredByPartyId });
                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, offeredByPartyId == 0 ? DBNull.Value : new List<int> { offeredByPartyId });
                pgcom.Parameters.AddWithValue("_resourceRegistryIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, resourceIds);
                pgcom.Parameters.AddWithValue("_resourceTypes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, new List<string> { resourceType.ToString().ToLower() });

                List<DelegationChange> receivedDelegations = new List<DelegationChange>();
                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    receivedDelegations.Add(GetResourceRegistryDelegationChange(reader));
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

        private static DelegationChange GetAppDelegationChange(NpgsqlDataReader reader)
        {
            return new DelegationChange
            {
                DelegationChangeId = reader.GetFieldValue<int>("delegationchangeid"),
                DelegationChangeType = reader.GetFieldValue<DelegationChangeType>("delegationchangetype"),
                ResourceId = reader.GetFieldValue<string>("altinnappid"),
                ResourceType = ResourceAttributeMatchType.AltinnAppId.ToString(),
                OfferedByPartyId = reader.GetFieldValue<int>("offeredbypartyid"),
                CoveredByPartyId = reader.GetFieldValue<int?>("coveredbypartyid"),
                CoveredByUserId = reader.GetFieldValue<int?>("coveredbyuserid"),
                PerformedByUserId = reader.GetFieldValue<int?>("performedbyuserid"),
                BlobStoragePolicyPath = reader.GetFieldValue<string>("blobstoragepolicypath"),
                BlobStorageVersionId = reader.GetFieldValue<string>("blobstorageversionid"),
                Created = reader.GetFieldValue<DateTime>("created")                
            };
        }

        private static DelegationChange GetResourceRegistryDelegationChange(NpgsqlDataReader reader)
        {
            return new DelegationChange
            {
                ResourceRegistryDelegationChangeId = reader.GetFieldValue<int>("resourceregistrydelegationchangeid"),
                DelegationChangeType = reader.GetFieldValue<DelegationChangeType>("delegationchangetype"),
                ResourceId = reader.GetFieldValue<string>("resourceregistryid"),
                ResourceType = reader.GetFieldValue<string>("resourcetype"),
                OfferedByPartyId = reader.GetFieldValue<int>("offeredbypartyid"),
                CoveredByPartyId = reader.GetFieldValue<int?>("coveredbypartyid"),
                CoveredByUserId = reader.GetFieldValue<int?>("coveredbyuserid"),
                PerformedByUserId = reader.GetFieldValue<int?>("performedbyuserid"),
                PerformedByPartyId = reader.GetFieldValue<int?>("performedbypartyid"),
                BlobStoragePolicyPath = reader.GetFieldValue<string>("blobstoragepolicypath"),
                BlobStorageVersionId = reader.GetFieldValue<string>("blobstorageversionid"),
                Created = reader.GetFieldValue<DateTime>("created")
            };
        }

        private async Task<List<DelegationChange>> GetAllCurrentAppDelegationChangesCoveredByPartyIds(List<string> altinnAppIds = null, List<int> offeredByPartyIds = null, List<int> coveredByPartyIds = null)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesForCoveredByPartyIds, conn);
                pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, offeredByPartyIds);
                pgcom.Parameters.AddWithValue("_coveredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, coveredByPartyIds);

                List<DelegationChange> delegationChanges = new List<DelegationChange>();

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    delegationChanges.Add(GetAppDelegationChange(reader));
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesForCoveredByUserIds, conn);
                pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, offeredByPartyIds);
                pgcom.Parameters.AddWithValue("_coveredByUserIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, coveredByUserIds);

                List<DelegationChange> delegationChanges = new List<DelegationChange>();

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    delegationChanges.Add(GetAppDelegationChange(reader));
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
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(getAppDelegationChangesOfferedByPartyIds, conn);
                pgcom.Parameters.AddWithValue("_altinnAppIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, altinnAppIds?.Count > 0 ? altinnAppIds : DBNull.Value);
                pgcom.Parameters.AddWithValue("_offeredByPartyIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, offeredByPartyIds);

                List<DelegationChange> delegationChanges = new List<DelegationChange>();

                using NpgsqlDataReader reader = pgcom.ExecuteReader();
                while (reader.Read())
                {
                    delegationChanges.Add(GetAppDelegationChange(reader));
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
}
