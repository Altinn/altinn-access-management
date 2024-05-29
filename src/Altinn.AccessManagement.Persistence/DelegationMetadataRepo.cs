using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Telemetry;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Repository implementation for PostgreSQL operations on delegations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DelegationMetadataRepo : IDelegationMetadataRepository
    {
        private readonly IDbConnection _connection;
        private readonly string defaultColumns = "delegationChangeId, delegationChangeType, altinnAppId, offeredByPartyId, coveredByUserId, coveredByPartyId, performedByUserId, blobStoragePolicyPath, blobStorageVersionId, created";

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationMetadataRepo"/> class
        /// </summary>
        /// <param name="dbConnection">Database connection for AuthorizationDb</param>
        public DelegationMetadataRepo(NpgsqlDataSource dbConnection)
        {
            var bld = new NpgsqlConnectionStringBuilder(dbConnection.ConnectionString);
            bld.AutoPrepareMinUsages = 2;
            bld.MaxAutoPrepare = 50;
            _connection = new Npgsql.NpgsqlConnection(bld.ConnectionString);
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("altinnAppId", altinnAppId);
            param.Add("offeredByPartyId", offeredByPartyId);

            string query = $"SELECT {defaultColumns}" +
            "FROM delegation.delegationChanges" +
            "WHERE altinnAppId = @altinnAppId AND offeredByPartyId = @offeredByPartyId";

            if (coveredByPartyId != null)
            {
                query += " AND coveredByPartyId = @coveredByPartyId";
                param.Add("coveredByPartyId", coveredByPartyId);
            }

            if (coveredByUserId != null)
            {
                query += " AND coveredByUserId = @coveredByUserId";
                param.Add("coveredByUserId", coveredByUserId);
            }

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (offeredByPartyIds?.Any() != true)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(offeredByPartyIds)));
                throw new ArgumentNullException(nameof(offeredByPartyIds));
            }

            var param = new Dictionary<string, object>();

            string query = "WITH latestChanges AS (" +
            "SELECT MAX(delegationChangeId) as latestId" +
            "FROM delegation.delegationchanges" +
            "WHERE 1=1";

            if (altinnAppIds != null && altinnAppIds.Count != 0)
            {
                query += " AND (altinnAppId IN(@altinnAppIds))";
                param.Add("altinnAppIds", altinnAppIds);
            }

            if (offeredByPartyIds != null && offeredByPartyIds.Count != 0)
            {
                query += " AND (offeredByPartyId IN(@offeredByPartyIds))";
                param.Add("offeredByPartyIds", offeredByPartyIds);
            }

            if (coveredByPartyIds != null && coveredByPartyIds.Count != 0)
            {
                query += " AND (coveredByPartyId IN(@coveredByPartyIds))";
                param.Add("coveredByPartyIds", coveredByPartyIds);
            }

            if (coveredByUserIds != null && coveredByUserIds.Count != 0)
            {
                query += " AND (coveredByUserId IN(@coveredByUserIds))";
                param.Add("coveredByUserIds", coveredByUserIds);
            }

            query += "GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId, coveredByUserId" +
            ")" + 
            $"SELECT {defaultColumns}" +
            "FROM delegation.delegationchanges" +
            "INNER JOIN latestChanges ON delegationchangeid = latestChanges.latestId";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally 
            { 
                _connection.Close(); 
            }
        }

        /// <inheritdoc/>
        public async Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
        {
            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                return await GetCurrentAppDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, cancellationToken);
            }

            return await GetCurrentResourceRegistryDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, cancellationToken);
        }

        private async Task<DelegationChange> GetCurrentAppDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("altinnAppId", resourceId);
            param.Add("offeredByPartyId", offeredByPartyId);

            string query = $"SELECT {defaultColumns}" +
                    "FROM delegation.delegationChanges" +
                    "WHERE altinnAppId = @altinnAppId AND offeredByPartyId = @offeredByPartyId";

            if (coveredByPartyId != null)
            {
                query += " AND coveredByPartyId = @coveredByPartyId";
                param.Add("coveredByPartyId", coveredByPartyId);
            }

            if (coveredByUserId != null)
            {
                query += " AND coveredByUserId = @coveredByUserId";
                param.Add("coveredByUserId", coveredByUserId);
            }

            query += "ORDER BY delegationChangeId DESC LIMIT 1";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                return await InsertAppDelegation(delegationChange);
            }

            return await InsertResourceRegistryDelegation(delegationChange);
        }

        private async Task<DelegationChange> InsertAppDelegation(DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>
            {
                { "delegationChangeType", delegationChange.DelegationChangeType },
                { "altinnAppId", delegationChange.ResourceId },
                { "offeredByPartyId", delegationChange.OfferedByPartyId },
                { "coveredByUserId", delegationChange.CoveredByUserId.HasValue ? delegationChange.CoveredByUserId.Value : DBNull.Value },
                { "coveredByPartyId", delegationChange.CoveredByPartyId.HasValue ? delegationChange.CoveredByPartyId.Value : DBNull.Value },
                { "performedByUserId", delegationChange.PerformedByUserId.HasValue ? delegationChange.PerformedByUserId.Value : DBNull.Value },
                { "blobStoragePolicyPath", delegationChange.BlobStoragePolicyPath },
                { "blobStorageVersionId", delegationChange.BlobStorageVersionId }
            };

            string query = $"INSERT INTO delegation.delegationChanges(delegationChangeType, altinnAppId, offeredByPartyId, coveredByUserId, coveredByPartyId, performedByUserId, blobStoragePolicyPath, blobStorageVersionId)" +
                $"VALUES (@delegationChangeType, @altinnAppId, @offeredByPartyId, @coveredByUserId, @coveredByPartyId, @performedByUserId, @blobStoragePolicyPath, @blobStorageVersionId)" +
                $"RETURNING *;";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private async Task<DelegationChange> InsertResourceRegistryDelegation(DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            var param = new Dictionary<string, object>
            {
                { "delegationChangeType", delegationChange.DelegationChangeType },
                { "altinnAppId", delegationChange.ResourceId }, // resourceregistryid ?
                { "offeredByPartyId", delegationChange.OfferedByPartyId },
                { "coveredByUserId", delegationChange.CoveredByUserId.HasValue ? delegationChange.CoveredByUserId.Value : DBNull.Value },
                { "coveredByPartyId", delegationChange.CoveredByPartyId.HasValue ? delegationChange.CoveredByPartyId.Value : DBNull.Value },
                { "performedByUserId", delegationChange.PerformedByUserId.HasValue ? delegationChange.PerformedByUserId.Value : DBNull.Value },
                { "performedByPartyId", delegationChange.PerformedByPartyId.HasValue ? delegationChange.PerformedByUserId.Value : DBNull.Value },
                { "blobStoragePolicyPath", delegationChange.BlobStoragePolicyPath },
                { "blobStorageVersionId", delegationChange.BlobStorageVersionId },
                { "delegatedTime", delegationChange.Created } // check ?
            };

            string query = $"WITH insertRow AS (" +
                $"SELECT @delegationChangeType, R.resourceId, @offeredByPartyId, @coveredByUserId, @coveredByPartyId, @performedByUserId, @performedByPartyId, @blobStoragePolicyPath, @blobStorageVersionId, @delegatedTime" +
                $"FROM accessmanagement.Resource AS R" +
                $"WHERE resourceRegistryId = @resourceregistryid" +
                $"), insertAction AS (" +
                $"INSERT INTO delegation.ResourceRegistryDelegationChanges" +
                $"(delegationChangeType, resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId, performedByUserId, performedByPartyId, blobStoragePolicyPath, blobStorageVersionId, created)" +
                $"SELECT delegationChangeType, resourceId, offeredByPartyId, coveredByUserId, coveredByPartyId, performedByUserId, performedByPartyId, blobStoragePolicyPath, blobStorageVersionId, delegatedTime" +
                $"FROM insertRow" +
                $")" +
                $"SELECT * FROM insertRow";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private async Task<DelegationChange> GetCurrentResourceRegistryDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("resourceRegistryId", resourceId);
            param.Add("offeredByPartyId", offeredByPartyId);

            string query = "SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId, res.resourceType, rr.offeredByPartyId, rr.coveredByUserId, rr.coveredByPartyId, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created" +
            "FROM delegation.ResourceRegistryDelegationChanges AS rr" +
            "JOIN accessmanagement.Resource AS res ON rr.resourceId_fk = res.resourceid" +
            "WHERE res.resourceRegistryId = @resourceRegistryId AND offeredByPartyId = @offeredByPartyId";

            if (coveredByUserId.HasValue)
            {
                query += " AND coveredByUserId = @coveredByUserId";
                param.Add("coveredByUserId", coveredByUserId.Value);
            }

            if (coveredByPartyId.HasValue)
            {
                query += " AND coveredByPartyId = @coveredByPartyId";
                param.Add("coveredByPartyId", coveredByPartyId.Value);
            }

            query += "ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default)
        {
            if (offeredByPartyIds?.Any() != true)
            {
                throw new ArgumentNullException(nameof(offeredByPartyIds));
            }

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

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
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("offeredByPartyId", offeredByPartyId);

            string query = "WITH lastChange AS (" +
                "SELECT MAX(DC.resourceRegistryDelegationChangeId) AS changeId, R.resourceId, R.resourceRegistryId, R.resourceType" +
                "FROM accessmanagement.Resource AS R" +
                "INNER JOIN delegation.ResourceRegistryDelegationChanges AS DC ON R.resourceid = DC.resourceid_fk" +
                "WHERE DC.offeredByPartyId = @offeredByPartyId";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += "AND resourceRegistryId IN (@resourceRegistryIds)";
                param.Add("resourceRegistryIds", resourceRegistryIds);
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += "AND resourceType IN (@resourceTypes)";
                param.Add("resourceTypes", resourceTypes.Select(t => t.ToString().ToLower()).ToList());
            }

            query += "GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId, R.resourceId, R.resourceRegistryId, R.resourceType)";

            query += "SELECT" +
            "change.resourceRegistryDelegationChangeId, change.delegationChangeType, lastChange.resourceRegistryId, lastChange.resourceType, change.offeredByPartyId, change.coveredByUserId, change.coveredByPartyId, change.performedByUserId, change.performedByPartyId, change.blobStoragePolicyPath, change.blobStorageVersionId, change.created" +
            "FROM delegation.ResourceRegistryDelegationChanges AS change" +
            "INNER JOIN lastChange ON change.resourceId_fk = lastChange.resourceid AND change.resourceRegistryDelegationChangeId = lastChange.changeId";
            query += "WHERE delegationchangetype != 'revoke_last'";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();

            string query = "WITH res AS (" +
                "SELECT resourceId, resourceRegistryId, resourceType" +
                "FROM accessmanagement.Resource" +
                "WHERE 1=1";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += " AND resourceRegistryId IN (@resourceRegistryIds)";
                param.Add("resourceRegistryIds", resourceRegistryIds);
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += " AND resourceType IN (@resourceTypes)";
                param.Add("resourceTypes", resourceTypes);
            }

            query += "), active AS (" +
            "SELECT MAX(resourceRegistryDelegationChangeId) AS changeId" +
            "FROM delegation.ResourceRegistryDelegationChanges AS rrdc" +
            "INNER JOIN res ON rrdc.resourceId_fk = res.resourceid" +
            "WHERE 1=1";
            
            if (offeredByPartyIds != null && offeredByPartyIds.Count > 0)
            {
                query += "AND offeredByPartyId IN (@offeredByPartyIds)";
                param.Add("offeredByPartyIds", offeredByPartyIds);
            }

            if (coveredByPartyIds != null && coveredByPartyIds.Count > 0)
            {
                // TODO: Check if not optional?
                query += "AND coveredByPartyId IN (@coveredByPartyIds)";
                param.Add("coveredByPartyIds", coveredByPartyIds);
            }
                
            query += "GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId )" +
            "SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId, res.resourceType, rr.offeredByPartyId, rr.coveredByUserId, rr.coveredByPartyId, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created" +
            "FROM delegation.ResourceRegistryDelegationChanges AS rr" +
            "INNER JOIN res ON rr.resourceId_fk = res.resourceid" +
            "INNER JOIN active ON rr.resourceRegistryDelegationChangeId = active.changeId" +
            "WHERE delegationchangetype != 'revoke_last'";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();

            string query = "WITH res AS (" +
                "SELECT resourceId, resourceRegistryId, resourceType" +
                "FROM accessmanagement.Resource" +
                "WHERE 1=1";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += " AND resourceRegistryId IN (@resourceRegistryIds)";
                param.Add("resourceRegistryIds", resourceRegistryIds);
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += " AND resourceType IN (@resourceTypes)";
                param.Add("resourceTypes", resourceTypes);
            }

            query += "), active AS (" +
            "SELECT MAX(resourceRegistryDelegationChangeId) AS changeId" +
            "FROM delegation.ResourceRegistryDelegationChanges AS rrdc" +
            "INNER JOIN res ON rrdc.resourceId_fk = res.resourceid" +
            "WHERE 1=1";

            if (offeredByPartyIds != null && offeredByPartyIds.Count > 0)
            {
                query += "AND offeredByPartyId IN (@offeredByPartyIds)";
                param.Add("offeredByPartyIds", offeredByPartyIds);
            }

            if (coveredByUserId > 0)
            {
                // TODO: Check if not optional?
                query += "AND coveredByUserId = @coveredByUserId";
                param.Add("coveredByUserId", coveredByUserId);
            }

            query += "GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId )" +
            "SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId, res.resourceType, rr.offeredByPartyId, rr.coveredByUserId, rr.coveredByPartyId, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created" +
            "FROM delegation.ResourceRegistryDelegationChanges AS rr" +
            "INNER JOIN res ON rr.resourceId_fk = res.resourceid" +
            "INNER JOIN active ON rr.resourceRegistryDelegationChangeId = active.changeId" +
            "WHERE delegationchangetype != 'revoke_last'";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();

            string query = "WITH lastChange AS (" +
                "SELECT MAX(resourceRegistryDelegationChangeId) AS changeId, R.resourceId, R.resourceRegistryId, R.resourceType" +
                "FROM accessmanagement.Resource AS R" +
                "INNER JOIN delegation.ResourceRegistryDelegationChanges AS DC ON DC.resourceId_fk = R.resourceid" +
                "WHERE 1=1";

            if (offeredByPartyId > 0)
            {
                // TODO: Check if not optional?
                query += "AND offeredByPartyId = @offeredByPartyId";
                param.Add("offeredByPartyId", offeredByPartyId);
            }

            if (coveredByPartyId > 0) 
            {
                // TODO: Check if not optional?
                query += "AND coveredByPartyId = @coveredByPartyId";
                param.Add("coveredByPartyId", coveredByPartyId);
            }

            if (resourceType > 0)
            {
                // TODO: Check if not optional?
                query += "AND R.resourceType = @resourceType";
                param.Add("resourceType", resourceType.ToString().ToLower());
            }

            if (resourceIds.Count > 0)
            {
                // TODO: Check if not optional?
                query += "AND R.resourceRegistryId IN (@resourceRegistryIds)";
                param.Add("resourceRegistryIds", resourceIds);
            }

            query += "GROUP BY DC.resourceId_fk, DC.offeredByPartyId, DC.coveredByPartyId, DC.coveredByUserId, R.resourceId, R.resourceRegistryId, R.resourceType )";

            query += "SELECT" +
                "change.resourceRegistryDelegationChangeId, change.delegationChangeType, lastChange.resourceRegistryId, lastChange.resourceType, " +
                "change.offeredByPartyId, change.coveredByUserId change.coveredByPartyId, change.performedByUserId change.performedByPartyId, " +
                "change.blobStoragePolicyPath, change.blobStorageVersionId, change.created" +
                "FROM delegation.ResourceRegistryDelegationChanges AS change" +
                "INNER JOIN lastChange ON change.resourceId_fk = lastChange.resourceid" +
                "WHERE delegationchangetype != 'revoke_last'";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("offeredByPartyIds", offeredByPartyIds);

            const string query = /*strpsql*/@"
            WITH resources AS (
		        SELECT resourceId, resourceRegistryId, resourceType
		        FROM accessmanagement.Resource
		        WHERE resourceType != 'maskinportenschema'
	        ),
            latestResourceChanges AS (
		        SELECT MAX(resourceRegistryDelegationChangeId) as latestId
		        FROM delegation.ResourceRegistryDelegationChanges
                WHERE offeredbypartyid IN (@offeredByPartyIds)
                GROUP BY resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId
	        ),
	        latestAppChanges AS (
		        SELECT MAX(delegationChangeId) as latestId
		        FROM delegation.delegationchanges 
                WHERE offeredbypartyid IN (@offeredByPartyIds)
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
	        WHERE delegationchangetype != 'revoke_last'";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (coveredByUserIds == null && coveredByPartyIds == null)
            {
                return new List<DelegationChange>(); // await Task.FromResult(new List<DelegationChange>());
            }

            var param = new Dictionary<string, object>();
            param.Add("coveredByUserIds", coveredByUserIds);
            param.Add("coveredByPartyIds", coveredByPartyIds);

            const string query = /*strpsql*/@"
            WITH resources AS (
		        SELECT resourceId, resourceRegistryId, resourceType
		        FROM accessmanagement.Resource
		        WHERE resourceType != 'maskinportenschema'
	        ),
	        latestResourceChanges AS (
		        SELECT MAX(resourceRegistryDelegationChangeId) as latestId
		        FROM delegation.ResourceRegistryDelegationChanges
		        WHERE 
			        coveredByUserId IN (@coveredbyuserids)
			        OR coveredByPartyId IN (@coveredByPartyIds)
		        GROUP BY resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId
	        ),
	        latestAppChanges AS (
		        SELECT MAX(delegationChangeId) as latestId
		        FROM delegation.delegationchanges
		        WHERE
			        coveredByUserId IN (@coveredByUserIds)
			        OR coveredByPartyId IN (@coveredByPartyIds)
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
                _connection.Open();
                var res = await _connection.QueryAsync<DelegationChange>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res == null ? null : res.ToList();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
