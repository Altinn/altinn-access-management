using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
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
        private readonly NpgsqlDataSource _conn;
        private readonly string defaultAppColumns = "delegationChangeId, delegationChangeType, altinnAppId, offeredByPartyId, fromUuid, fromType, coveredByUserId, coveredByPartyId, toUuid, toType, performedByUserId, performedByUuid, performedByType, blobStoragePolicyPath, blobStorageVersionId, created";
        private const string FromUuid = "fromUuid";
        private const string FromType = "fromType";
        private const string ToUuid = "toUuid";
        private const string ToType = "toType";
        private const string InstanceId = "instanceId";
        private const string ResourceId = "resourceId";
        private const string DelegationChangeType = "delegationChangeType";

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationMetadataRepo"/> class
        /// </summary>
        /// <param name="conn">PostgreSQL datasource connection</param>
        public DelegationMetadataRepo(NpgsqlDataSource conn)
        {
            _conn = conn;
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (coveredByUserId == null && coveredByPartyId == null)
            {
                activity?.StopWithError(new ArgumentException($"Both params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)} cannot be null."));
                throw new ArgumentException($"Both params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)} cannot be null.");
            }

            string query = string.Empty;
            if (coveredByPartyId != null)
            {
                query = /*strpsql*/@$"
                SELECT {defaultAppColumns}
                FROM delegation.delegationChanges
                WHERE altinnAppId = @altinnAppId
                    AND offeredByPartyId = @offeredByPartyId
                    AND coveredByPartyId = @coveredByPartyId
                ";
            }

            if (coveredByUserId != null)
            {
                query = /*strpsql*/@$"
                SELECT {defaultAppColumns}
                FROM delegation.delegationChanges
                WHERE altinnAppId = @altinnAppId
                    AND offeredByPartyId = @offeredByPartyId
                    AND coveredByUserId = @coveredByUserId
                ";
            }

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("altinnAppId", NpgsqlDbType.Text, altinnAppId);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, offeredByPartyId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, coveredByPartyId);
                cmd.Parameters.AddWithNullableValue("coveredByUserId", NpgsqlDbType.Integer, coveredByUserId);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetAppDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            if (offeredByPartyIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(offeredByPartyIds)));
                throw new ArgumentNullException(nameof(offeredByPartyIds));
            }

            if (altinnAppIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(altinnAppIds)));
                throw new ArgumentNullException(nameof(altinnAppIds));
            }

            if (coveredByPartyIds == null && coveredByUserIds == null)
            {
                activity?.StopWithError(new ArgumentException($"Both params: {nameof(coveredByUserIds)}, {nameof(coveredByPartyIds)} cannot be null."));
                throw new ArgumentException($"Both params: {nameof(coveredByUserIds)}, {nameof(coveredByPartyIds)} cannot be null.");
            }

            string query = string.Empty;
            if (coveredByPartyIds?.Count > 0)
            {
                query = /*strpsql*/@$"
                WITH latestChanges AS (
                    SELECT MAX(delegationChangeId) as latestId
                    FROM delegation.delegationchanges
                    WHERE (offeredByPartyId = ANY (@offeredByPartyIds))
                        AND (altinnAppId = ANY (@altinnAppIds))
                        AND (coveredByPartyId = ANY (@coveredByPartyIds))
                    GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId, coveredByUserId
                )
                SELECT {defaultAppColumns}
                FROM delegation.delegationchanges
                    INNER JOIN latestChanges ON delegationchangeid = latestChanges.latestId
                ";
            }
            else if (coveredByUserIds?.Count > 0)
            {
                query = /*strpsql*/@$"
                WITH latestChanges AS (
                    SELECT MAX(delegationChangeId) as latestId
                    FROM delegation.delegationchanges
                    WHERE (offeredByPartyId = ANY (@offeredByPartyIds))
                        AND (altinnAppId = ANY (@altinnAppIds))
                        AND (coveredByUserId = ANY (@coveredByUserIds))
                    GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId, coveredByUserId
                )
                SELECT {defaultAppColumns}
                FROM delegation.delegationchanges
                    INNER JOIN latestChanges ON delegationchangeid = latestChanges.latestId
                ";
            }

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);
                cmd.Parameters.AddWithValue("altinnAppIds", NpgsqlDbType.Array | NpgsqlDbType.Text, altinnAppIds);
                cmd.Parameters.AddWithNullableValue("coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);
                cmd.Parameters.AddWithNullableValue("coveredByUserIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByUserIds);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetAppDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<string> altinnAppIds, List<int> fromPartyIds, UuidType toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            if (altinnAppIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(altinnAppIds)));
                throw new ArgumentNullException(nameof(altinnAppIds));
            }

            if (fromPartyIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(fromPartyIds)));
                throw new ArgumentNullException(nameof(fromPartyIds));
            }

            if (toUuidType == UuidType.NotSpecified)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(toUuidType)} must be specified."));
                throw new ArgumentException($"Param: {nameof(toUuidType)} must be specified.");
            }

            if (toUuid == Guid.Empty)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(toUuid)} must be specified."));
                throw new ArgumentException($"Param: {nameof(toUuid)} must be specified.");
            }

            string query = /*strpsql*/@$"
            WITH latestChanges AS (
                SELECT MAX(delegationChangeId) as latestId
                FROM delegation.delegationchanges
                WHERE offeredByPartyId = ANY (@offeredByPartyIds)
                    AND altinnAppId = ANY (@altinnAppIds)
                    AND toType = @toType
                    AND toUuid = @toUuid
                GROUP BY altinnAppId, offeredByPartyId, offeredByPartyId, toType, toUuid
            )
            SELECT {defaultAppColumns}
            FROM delegation.delegationchanges
                INNER JOIN latestChanges ON delegationchangeid = latestChanges.latestId
            WHERE delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("altinnAppIds", NpgsqlDbType.Array | NpgsqlDbType.Text, altinnAppIds);
                cmd.Parameters.AddWithValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, fromPartyIds);
                cmd.Parameters.AddWithValue(ToType, toUuidType);
                cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, toUuid);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetAppDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default)
        {
            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                return await GetCurrentAppDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, cancellationToken);
            }

            return await GetCurrentResourceRegistryDelegation(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, cancellationToken);
        }

        private async Task<DelegationChange> GetCurrentAppDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace."));
                throw new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace.");
            }

            if (offeredByPartyId == 0)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero."));
                throw new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero.");
            }

            if (coveredByPartyId == null && coveredByUserId == null && toUuidType != UuidType.SystemUser)
            {
                activity?.StopWithError(new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null."));
                throw new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null.");
            }

            string query = string.Empty;
            if (coveredByPartyId != null)
            {
                query = /*strpsql*/@$"
                SELECT {defaultAppColumns}
                FROM delegation.delegationChanges
                WHERE altinnAppId = @altinnAppId AND offeredByPartyId = @offeredByPartyId
                    AND coveredByPartyId = @coveredByPartyId
                ORDER BY delegationChangeId DESC LIMIT 1
                ";
            }

            if (coveredByUserId != null)
            {
                query = /*strpsql*/@$"
                SELECT {defaultAppColumns}
                FROM delegation.delegationChanges
                WHERE altinnAppId = @altinnAppId AND offeredByPartyId = @offeredByPartyId
                    AND coveredByUserId = @coveredByUserId
                ORDER BY delegationChangeId DESC LIMIT 1
                ";
            }

            if (toUuidType == UuidType.SystemUser)
            {
                query = /*strpsql*/@$"
                SELECT {defaultAppColumns}
                FROM delegation.delegationChanges
                WHERE
                    altinnAppId = @altinnAppId
                    AND offeredByPartyId = @offeredByPartyId
                    AND toUuid = @toUuid
                    AND toType = @toType
                ORDER BY delegationChangeId DESC LIMIT 1
                ";
            }

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, offeredByPartyId);
                cmd.Parameters.AddWithValue("altinnAppId", NpgsqlDbType.Text, resourceId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, coveredByPartyId);
                cmd.Parameters.AddWithNullableValue("coveredByUserId", NpgsqlDbType.Integer, coveredByUserId);
                cmd.Parameters.AddWithNullableValue(ToUuid, NpgsqlDbType.Uuid, toUuid);
                cmd.Parameters.AddWithValue(ToType, toUuidType);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetAppDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                return await InsertAppDelegation(delegationChange, cancellationToken);
            }

            return await InsertResourceRegistryDelegation(delegationChange, cancellationToken);
        }

        /// <summary>
        ///  Fetches all instance delegated to given param
        /// </summary>
        /// <param name="toUuid">list of parties that has received an instance delegation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public async Task<List<InstanceDelegationChange>> GetAllCurrentReceivedInstanceDelegations(List<Guid> toUuid, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var query = /* strpsql */ @"
                WITH latestChanges AS (
                    SELECT
                        MAX(instancedelegationchangeid) as latestId
                    FROM
                        delegation.instancedelegationchanges
                    WHERE
                        touuid = ANY(@toUuid)
                    GROUP BY
                        touuid,
                        fromuuid,
                        resourceid,
                        instanceid
                )
                SELECT
                    instancedelegationchangeid,
                    delegationchangetype,
                    instanceDelegationMode,
                    resourceid,
                    instanceid,
                    fromuuid,
                    fromtype,
                    touuid,
                    totype,
                    performedby,
                    performedbytype,
                    blobstoragepolicypath,
                    blobstorageversionid,
                    created
                FROM
                    delegation.instancedelegationchanges
                INNER JOIN latestChanges
                    ON instancedelegationchangeid = latestChanges.latestId
                WHERE
                    delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("toUuid", NpgsqlDbType.Array | NpgsqlDbType.Uuid, toUuid);
                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetInstanceDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<InstanceDelegationChange> GetLastInstanceDelegationChange(InstanceDelegationChangeRequest request, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            SELECT
                instancedelegationchangeid
                ,delegationchangetype
                ,instanceDelegationMode
                ,resourceId
                ,instanceId
                ,fromUuid
                ,fromType
                ,toUuid
                ,toType
                ,performedBy
                ,performedByType
                ,blobStoragePolicyPath
                ,blobStorageVersionId
                ,created
            FROM
                delegation.instancedelegationchanges
            WHERE
                resourceId = @resourceId
                AND instanceId = @instanceId
                AND instanceDelegationMode = @instanceDelegationMode
                AND fromUuid = @fromUuid
                AND fromType = @fromType
                AND toUuid = @toUuid
                AND toType = @toType
            ORDER BY
                instancedelegationchangeid DESC LIMIT 1;
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, request.Resource);
                cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, request.Instance);
                cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationMode>("instancedelegationmode", request.InstanceDelegationMode));
                cmd.Parameters.AddWithValue(FromUuid, NpgsqlDbType.Uuid, request.FromUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType>(FromType, request.FromType));
                cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, request.ToUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType>(ToType, request.ToType));

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetInstanceDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<InstanceDelegationChange> InsertInstanceDelegation(InstanceDelegationChange instanceDelegationChange, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            INSERT INTO delegation.instancedelegationchanges(
                delegationchangetype
                ,instanceDelegationMode
                ,resourceId
                ,instanceid
                ,fromUuid
                ,fromType
                ,toUuid
                ,toType
                ,performedBy
                ,performedByType
                ,blobStoragePolicyPath
                ,blobStorageVersionId)
            VALUES (
                @delegationchangetype
                ,@instanceDelegationMode
                ,@resourceId
                ,@instanceid
                ,@fromUuid
                ,@fromType
                ,@toUuid
                ,@toType
                ,@performedBy
                ,@performedByType
                ,@blobStoragePolicyPath
                ,@blobStorageVersionId)
            RETURNING *;
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.Add(new NpgsqlParameter<DelegationChangeType>(DelegationChangeType, instanceDelegationChange.DelegationChangeType));
                cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationMode>("instancedelegationmode", instanceDelegationChange.InstanceDelegationMode));
                cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, instanceDelegationChange.ResourceId);
                cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, instanceDelegationChange.InstanceId);
                cmd.Parameters.AddWithValue(FromUuid, NpgsqlDbType.Uuid, instanceDelegationChange.FromUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(FromType, instanceDelegationChange.FromUuidType != UuidType.NotSpecified ? instanceDelegationChange.FromUuidType : null));
                cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, instanceDelegationChange.ToUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(ToType, instanceDelegationChange.ToUuidType != UuidType.NotSpecified ? instanceDelegationChange.ToUuidType : null));
                cmd.Parameters.AddWithValue("performedBy", NpgsqlDbType.Text, instanceDelegationChange.PerformedBy);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>("performedByType", instanceDelegationChange.PerformedByType != UuidType.NotSpecified ? instanceDelegationChange.PerformedByType : null));
                cmd.Parameters.AddWithValue("blobStoragePolicyPath", NpgsqlDbType.Text, instanceDelegationChange.BlobStoragePolicyPath);
                cmd.Parameters.AddWithValue("blobStorageVersionId", NpgsqlDbType.Text, instanceDelegationChange.BlobStorageVersionId);

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetInstanceDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<InstanceDelegationChange>> GetAllLatestInstanceDelegationChanges(InstanceDelegationSource source, string resourceID, string instanceID, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            WITH LatestChanges AS(
		    SELECT 
			    MAX(instancedelegationchangeid) instancedelegationchangeid
		    FROM
			    delegation.instancedelegationchanges
		    WHERE
			    instancedelegationsource = @source
                AND resourceid = @resourceId
			    AND instanceid = @instanceId
		    GROUP BY
			    instancedelegationmode
			    ,resourceid
			    ,instanceid
			    ,fromuuid
			    ,fromtype
			    ,touuid
			    ,totype)
            SELECT
	            dc.instancedelegationchangeid
	            ,delegationchangetype
	            ,instancedelegationmode
	            ,resourceid
	            ,instanceid
	            ,fromuuid
	            ,fromtype
	            ,touuid
	            ,totype
	            ,performedby
	            ,performedbytype
	            ,blobstoragepolicypath
	            ,blobstorageversionid
	            ,created
            FROM
	            LatestChanges lc
	            JOIN delegation.instancedelegationchanges dc ON lc.instancedelegationchangeid = dc.instancedelegationchangeid
            WHERE
                delegationchangetype != 'revoke_last';";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationSource>("source", source));
                cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, resourceID);
                cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, instanceID);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetInstanceDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InstanceDelegationChange>> GetActiveInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            WITH LatestChanges AS(
		    SELECT 
			    MAX(instancedelegationchangeid) instancedelegationchangeid
		    FROM
			    delegation.instancedelegationchanges
		    WHERE
                resourceid = ANY(@resourceIds)
			    AND fromuuid = @fromUuid
                AND touuid = ANY(@toUuids)
		    GROUP BY
			    instancedelegationmode
			    ,resourceid
			    ,instanceid
			    ,fromuuid
			    ,fromtype
			    ,touuid
			    ,totype)
            SELECT
	            dc.instancedelegationchangeid
	            ,delegationchangetype
	            ,instancedelegationmode
	            ,resourceid
	            ,instanceid
	            ,fromuuid
	            ,fromtype
	            ,touuid
	            ,totype
	            ,performedby
	            ,performedbytype
	            ,blobstoragepolicypath
	            ,blobstorageversionid
	            ,created
            FROM
	            LatestChanges lc
	            JOIN delegation.instancedelegationchanges dc ON lc.instancedelegationchangeid = dc.instancedelegationchangeid
            WHERE delegationchangetype != 'revoke_last';";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("resourceIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceIds);
                cmd.Parameters.AddWithValue("fromUuid", NpgsqlDbType.Uuid, from);
                cmd.Parameters.AddWithValue("toUuids", NpgsqlDbType.Array | NpgsqlDbType.Uuid, to);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetInstanceDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        private static async ValueTask<InstanceDelegationChange> GetInstanceDelegationChange(NpgsqlDataReader reader)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                InstanceDelegationChange result = new()
                {
                    InstanceDelegationChangeId = await reader.GetFieldValueAsync<int>("instancedelegationchangeid"),
                    DelegationChangeType = await reader.GetFieldValueAsync<DelegationChangeType>(DelegationChangeType),
                    InstanceDelegationMode = await reader.GetFieldValueAsync<InstanceDelegationMode>("instancedelegationmode"),
                    ResourceId = await reader.GetFieldValueAsync<string>(ResourceId),
                    InstanceId = await reader.GetFieldValueAsync<string>(InstanceId),
                    FromUuid = await reader.GetFieldValueAsync<Guid>(FromUuid),
                    FromUuidType = await reader.GetFieldValueAsync<UuidType?>(FromType) ?? UuidType.NotSpecified,
                    ToUuid = await reader.GetFieldValueAsync<Guid>(ToUuid),
                    ToUuidType = await reader.GetFieldValueAsync<UuidType?>(ToType) ?? UuidType.NotSpecified,
                    PerformedBy = await reader.GetFieldValueAsync<string>("performedBy"),
                    PerformedByType = await reader.GetFieldValueAsync<UuidType?>("performedByType") ?? UuidType.NotSpecified,
                    BlobStoragePolicyPath = await reader.GetFieldValueAsync<string>("blobstoragepolicypath"),
                    BlobStorageVersionId = await reader.GetFieldValueAsync<string>("blobstorageversionid"),
                    Created = await reader.GetFieldValueAsync<DateTime>("created")
                };

                return result;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                return await new ValueTask<InstanceDelegationChange>(Task.FromException<InstanceDelegationChange>(ex));
            }
        }

        private async Task<DelegationChange> InsertAppDelegation(DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            INSERT INTO delegation.delegationChanges(delegationChangeType, altinnAppId, offeredByPartyId, fromUuid, fromType, coveredByUserId, coveredByPartyId, toUuid, toType, performedByUserId, blobStoragePolicyPath, blobStorageVersionId)
            VALUES (@delegationChangeType, @altinnAppId, @offeredByPartyId, @fromUuid, @fromType, @coveredByUserId, @coveredByPartyId, @toUuid, @toType, @performedByUserId, @blobStoragePolicyPath, @blobStorageVersionId)
            RETURNING *;
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue(DelegationChangeType, delegationChange.DelegationChangeType);
                cmd.Parameters.AddWithValue("altinnAppId", NpgsqlDbType.Text, delegationChange.ResourceId);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, delegationChange.OfferedByPartyId);
                cmd.Parameters.AddWithNullableValue(FromUuid, NpgsqlDbType.Uuid, delegationChange.FromUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(FromType, delegationChange.FromUuidType != UuidType.NotSpecified ? delegationChange.FromUuidType : null));
                cmd.Parameters.AddWithNullableValue("coveredByUserId", NpgsqlDbType.Integer, delegationChange.CoveredByUserId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, delegationChange.CoveredByPartyId);
                cmd.Parameters.AddWithNullableValue(ToUuid, NpgsqlDbType.Uuid, delegationChange.ToUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(ToType, delegationChange.ToUuidType != UuidType.NotSpecified ? delegationChange.ToUuidType : null));
                cmd.Parameters.AddWithValue("performedByUserId", NpgsqlDbType.Integer, delegationChange.PerformedByUserId);
                cmd.Parameters.AddWithValue("blobStoragePolicyPath", NpgsqlDbType.Text, delegationChange.BlobStoragePolicyPath);
                cmd.Parameters.AddWithValue("blobStorageVersionId", NpgsqlDbType.Text, delegationChange.BlobStorageVersionId);

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetAppDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        private async Task<DelegationChange> InsertResourceRegistryDelegation(DelegationChange delegationChange, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            WITH insertRow AS (
                SELECT 
                @delegationChangeType AS delegationChangeType, 
                R.resourceId,
                R.resourceType,
                @offeredByPartyId AS offeredByPartyId,
                @fromUuid AS fromUuid, 
                @fromType AS fromType, 
                @coveredByUserId AS coveredByUserId, 
                @coveredByPartyId AS coveredByPartyId, 
                @toUuid AS toUuid, 
                @toType AS toType, 
                @performedByUserId AS performedByUserId, 
                @performedByPartyId AS performedByPartyId, 
                @blobStoragePolicyPath AS blobStoragePolicyPath, 
                @blobStorageVersionId AS blobStorageVersionId, 
                @delegatedTime AS delegatedTime
                FROM accessmanagement.Resource AS R 
                WHERE resourceRegistryId = @resourceregistryid
            ), insertAction AS (
                INSERT INTO delegation.ResourceRegistryDelegationChanges
                    (delegationChangeType, resourceId_fk, offeredByPartyId, fromUuid, fromType, coveredByUserId, coveredByPartyId, toUuid, toType, performedByUserId, performedByPartyId, blobStoragePolicyPath, blobStorageVersionId, created)
                SELECT delegationChangeType, resourceId, offeredByPartyId, fromUuid, fromType, coveredByUserId, coveredByPartyId, toUuid, toType, performedByUserId, performedByPartyId, blobStoragePolicyPath, blobStorageVersionId, delegatedTime
                FROM insertRow
                RETURNING *
            )
            SELECT
                ins.resourceRegistryDelegationChangeId,
                ins.delegationChangeType,
                @resourceregistryid AS resourceregistryid,
                insertRow.resourceType,
                ins.offeredByPartyId,
                ins.fromUuid,
                ins.fromType,
                ins.coveredByUserId,
                ins.coveredByPartyId,
                ins.toUuid,
                ins.toType,
                ins.performedByUserId,
                ins.performedByPartyId,
                ins.blobStoragePolicyPath,
                ins.blobStorageVersionId,	
                ins.created
              FROM insertAction AS ins
              JOIN insertRow ON ins.resourceId_fk = insertRow.resourceid;
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue(DelegationChangeType, delegationChange.DelegationChangeType);
                cmd.Parameters.AddWithValue("resourceregistryid", delegationChange.ResourceId);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, delegationChange.OfferedByPartyId);
                cmd.Parameters.AddWithNullableValue(FromUuid, NpgsqlDbType.Uuid, delegationChange.FromUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(FromType, delegationChange.FromUuidType != UuidType.NotSpecified ? delegationChange.FromUuidType : null));
                cmd.Parameters.AddWithNullableValue("coveredByUserId", NpgsqlDbType.Integer, delegationChange.CoveredByUserId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, delegationChange.CoveredByPartyId);
                cmd.Parameters.AddWithNullableValue(ToUuid, NpgsqlDbType.Uuid, delegationChange.ToUuid);
                cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(ToType, delegationChange.ToUuidType != UuidType.NotSpecified ? delegationChange.ToUuidType : null));
                cmd.Parameters.AddWithNullableValue("performedByUserId", NpgsqlDbType.Integer, delegationChange.PerformedByUserId);
                cmd.Parameters.AddWithNullableValue("performedByPartyId", NpgsqlDbType.Integer, delegationChange.PerformedByPartyId);
                cmd.Parameters.AddWithValue("blobStoragePolicyPath", NpgsqlDbType.Text, delegationChange.BlobStoragePolicyPath);
                cmd.Parameters.AddWithValue("blobStorageVersionId", NpgsqlDbType.Text, delegationChange.BlobStorageVersionId);
                cmd.Parameters.AddWithValue("delegatedTime", delegationChange.Created.HasValue ? delegationChange.Created.Value : DateTime.UtcNow);

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetResourceRegistryDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        private async Task<DelegationChange> GetCurrentResourceRegistryDelegation(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace."));
                throw new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace.");
            }

            if (offeredByPartyId == 0)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero."));
                throw new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero.");
            }

            if (coveredByPartyId == null && coveredByUserId == null && toUuidType != UuidType.SystemUser)
            {
                activity?.StopWithError(new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null."));
                throw new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null.");
            }

            string query = string.Empty;
            if (coveredByUserId.HasValue)
            {
                query = /*strpsql*/@"    
                SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId as resourceregistryid, res.resourceType, rr.offeredByPartyId, rr.fromUuid, rr.fromType, rr.coveredByUserId, rr.coveredByPartyId, rr.toUuid, rr.toType, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created
                FROM delegation.ResourceRegistryDelegationChanges AS rr
                JOIN accessmanagement.Resource AS res ON rr.resourceId_fk = res.resourceid
                WHERE res.resourceRegistryId = @resourceRegistryId AND offeredByPartyId = @offeredByPartyId
                    AND coveredByUserId = @coveredByUserId
                ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1
                ";
            }

            if (coveredByPartyId.HasValue)
            {
                query = /*strpsql*/@"    
                SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId as resourceregistryid, res.resourceType, rr.offeredByPartyId, rr.fromUuid, rr.fromType, rr.coveredByUserId, rr.coveredByPartyId, rr.toUuid, rr.toType, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created
                FROM delegation.ResourceRegistryDelegationChanges AS rr
                JOIN accessmanagement.Resource AS res ON rr.resourceId_fk = res.resourceid
                WHERE res.resourceRegistryId = @resourceRegistryId AND offeredByPartyId = @offeredByPartyId
                    AND coveredByPartyId = @coveredByPartyId
                ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1
                ";
            }

            if (toUuidType == UuidType.SystemUser)
            {
                query = /*strpsql*/@"    
                SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId as resourceregistryid, res.resourceType, rr.offeredByPartyId, rr.fromUuid, rr.fromType, rr.coveredByUserId, rr.coveredByPartyId, rr.toUuid, rr.toType, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created
                FROM 
                    delegation.ResourceRegistryDelegationChanges AS rr
                    JOIN accessmanagement.Resource AS res ON rr.resourceId_fk = res.resourceid
                WHERE 
                    res.resourceRegistryId = @resourceRegistryId 
                    AND offeredByPartyId = @offeredByPartyId
                    AND toUuid = @toUuid
                    AND toType = @toType
                ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1
                ";
            }

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, offeredByPartyId);
                cmd.Parameters.AddWithValue("resourceRegistryId", NpgsqlDbType.Text, resourceId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, coveredByPartyId);
                cmd.Parameters.AddWithNullableValue("coveredByUserId", NpgsqlDbType.Integer, coveredByUserId);
                cmd.Parameters.AddWithNullableValue(ToUuid, NpgsqlDbType.Uuid, toUuid);
                cmd.Parameters.AddWithValue(ToType, toUuidType);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetResourceRegistryDelegationChange(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default)
        {
            if (offeredByPartyIds?.Count == 0)
            {
                throw new ArgumentNullException(nameof(offeredByPartyIds));
            }

            List<DelegationChange> delegationChanges = new List<DelegationChange>();

            if (coveredByPartyIds?.Count > 0)
            {
                delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyIds, offeredByPartyIds, resourceRegistryIds, cancellationToken: cancellationToken));
            }

            if (coveredByUserId.HasValue)
            {
                delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByUser(coveredByUserId.Value, offeredByPartyIds, resourceRegistryIds, cancellationToken: cancellationToken));
            }

            return delegationChanges;
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<string> resourceRegistryIds, List<int> fromPartyIds, UuidType toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            if (resourceRegistryIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(resourceRegistryIds)));
                throw new ArgumentNullException(nameof(resourceRegistryIds));
            }

            if (fromPartyIds?.Count < 1)
            {
                activity?.StopWithError(new ArgumentNullException(nameof(fromPartyIds)));
                throw new ArgumentNullException(nameof(fromPartyIds));
            }

            if (toUuidType == UuidType.NotSpecified)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(toUuidType)} must be specified."));
                throw new ArgumentException($"Param: {nameof(toUuidType)} must be specified.");
            }

            if (toUuid == Guid.Empty)
            {
                activity?.StopWithError(new ArgumentException($"Param: {nameof(toUuid)} must be specified."));
                throw new ArgumentException($"Param: {nameof(toUuid)} must be specified.");
            }

            string query = /*strpsql*/@"    
            WITH resources AS (
                SELECT resourceId, resourceRegistryId, resourceType
                FROM accessmanagement.Resource
                WHERE resourceType != 'maskinportenschema'
                    AND resourceRegistryId = ANY (@resourceRegistryIds)
            ),
            latestResourceChanges AS (
                SELECT MAX(resourceRegistryDelegationChangeId) AS latestId
                FROM delegation.ResourceRegistryDelegationChanges
                WHERE offeredbypartyid = ANY (@offeredByPartyIds)
                    AND toType = @toType
                    AND toUuid = @toUuid
                GROUP BY resourceId_fk, offeredByPartyId, toType, toUuid
            )
            SELECT
                resourceRegistryDelegationChangeId,
                null AS delegationChangeId,
                delegationChangeType,
                resources.resourceRegistryId,
                resources.resourceType,
                null AS altinnAppId,
                offeredByPartyId,
                fromuuid,
                fromtype,
                coveredByUserId,
                coveredByPartyId,
                touuid,
                totype,
                performedByUserId,
                performedByPartyId,
                blobStoragePolicyPath,
                blobStorageVersionId,
                created
            FROM delegation.ResourceRegistryDelegationChanges
                INNER JOIN resources ON resourceId_fk = resources.resourceid
                INNER JOIN latestResourceChanges ON resourceRegistryDelegationChangeId = latestResourceChanges.latestId
            WHERE delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceRegistryIds);
                cmd.Parameters.AddWithValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, fromPartyIds);
                cmd.Parameters.AddWithValue(ToType, toUuidType);
                cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, toUuid);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetResourceRegistryDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            WITH lastChange AS (
                SELECT MAX(DC.resourceRegistryDelegationChangeId) AS changeId, R.resourceId, R.resourceRegistryId, R.resourceType
                FROM accessmanagement.Resource AS R
                INNER JOIN delegation.ResourceRegistryDelegationChanges AS DC ON R.resourceid = DC.resourceid_fk
                WHERE DC.offeredByPartyId = @offeredByPartyId 
            ";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceRegistryId = ANY (@resourceRegistryIds)";
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceType = ANY (@resourceTypes)";
            }

            query += /*strpsql*/@"
                GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId, R.resourceId, R.resourceRegistryId, R.resourceType
            )
            SELECT
                change.resourceRegistryDelegationChangeId,
                change.delegationChangeType,
                lastChange.resourceRegistryId AS resourceRegistryId,
                lastChange.resourceType,
                change.offeredByPartyId,
                change.fromUuid,
                change.fromType,
                change.coveredByUserId,
                change.coveredByPartyId,
                change.toUuid,
                change.toType,
                change.performedByUserId,
                change.performedByPartyId,
                change.blobStoragePolicyPath,
                change.blobStorageVersionId,
                change.created
            FROM delegation.ResourceRegistryDelegationChanges AS change
                INNER JOIN lastChange ON change.resourceId_fk = lastChange.resourceid AND change.resourceRegistryDelegationChangeId = lastChange.changeId
            WHERE delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("offeredByPartyId", NpgsqlDbType.Integer, offeredByPartyId);
                cmd.Parameters.AddWithNullableValue("resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceRegistryIds);
                cmd.Parameters.AddWithNullableValue("resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceTypes != null ? resourceTypes.Select(t => t.ToString().ToLower()).ToList() : null);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetResourceRegistryDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            WITH res AS (
                SELECT resourceId, resourceRegistryId, resourceType
                FROM accessmanagement.Resource
                WHERE 1=1
            ";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceRegistryId = ANY (@resourceRegistryIds)";
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceType = ANY (@resourceTypes)";
            }

            query += /*strpsql*/@"
            ), 
            active AS (
                SELECT MAX(resourceRegistryDelegationChangeId) AS changeId
                FROM delegation.ResourceRegistryDelegationChanges AS rrdc
                    INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
                WHERE coveredByPartyId = ANY (@coveredByPartyIds)
            ";

            if (offeredByPartyIds != null && offeredByPartyIds.Count > 0)
            {
                query += /*strpsql*/@"
                AND offeredByPartyId = ANY (@offeredByPartyIds)";
            }

            query += /*strpsql*/@"
                GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId 
            )
            SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId as resourceRegistryId, res.resourceType, rr.offeredByPartyId, rr.fromUuid, rr.fromType, rr.coveredByUserId, rr.coveredByPartyId, rr.ToUuid, rr.toType, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created
            FROM delegation.ResourceRegistryDelegationChanges AS rr
                INNER JOIN res ON rr.resourceId_fk = res.resourceid
                INNER JOIN active ON rr.resourceRegistryDelegationChangeId = active.changeId
            WHERE delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);
                cmd.Parameters.AddWithNullableValue("resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceRegistryIds);
                cmd.Parameters.AddWithNullableValue("resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceTypes != null ? resourceTypes.Select(t => t.ToString().ToLower()).ToList() : null);
                cmd.Parameters.AddWithNullableValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetResourceRegistryDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (coveredByUserId < 1)
            {
                throw new ArgumentException("CoveredByUserId is required");
            }

            string query = /*strpsql*/@"
            WITH res AS (
                SELECT resourceId, resourceRegistryId, resourceType
                FROM accessmanagement.Resource
                WHERE 1=1
            ";

            if (resourceRegistryIds != null && resourceRegistryIds.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceRegistryId = ANY (@resourceRegistryIds)";
            }

            if (resourceTypes != null && resourceTypes.Count > 0)
            {
                query += /*strpsql*/@"
                AND resourceType = ANY (@resourceTypes)";
            }

            query += /*strpsql*/@"
            ), 
            active AS (
                SELECT MAX(resourceRegistryDelegationChangeId) AS changeId
                FROM delegation.ResourceRegistryDelegationChanges AS rrdc
                    INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
                WHERE coveredByUserId = @coveredByUserId
            ";

            if (offeredByPartyIds != null && offeredByPartyIds.Count > 0)
            {
                query += /*strpsql*/" AND offeredByPartyId = ANY (@offeredByPartyIds)";
            }

            query += /*strpsql*/@"
                GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
            )
            SELECT rr.resourceRegistryDelegationChangeId, rr.delegationChangeType, res.resourceRegistryId as resourceRegistryId, res.resourceType, rr.offeredByPartyId, rr.fromUuid, rr.fromType,  rr.coveredByUserId, rr.coveredByPartyId, rr.toUuid, rr.toType, rr.performedByUserId, rr.performedByPartyId, rr.blobStoragePolicyPath, rr.blobStorageVersionId, rr.created
            FROM delegation.ResourceRegistryDelegationChanges AS rr
                INNER JOIN res ON rr.resourceId_fk = res.resourceid
                INNER JOIN active ON rr.resourceRegistryDelegationChangeId = active.changeId
            WHERE delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("coveredByUserId", NpgsqlDbType.Integer, coveredByUserId);
                cmd.Parameters.AddWithNullableValue("resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceRegistryIds);
                cmd.Parameters.AddWithNullableValue("resourceTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceTypes != null ? resourceTypes.Select(t => t.ToString().ToLower()).ToList() : null);
                cmd.Parameters.AddWithNullableValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetResourceRegistryDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            if (resourceIds.Count < 1)
            {
                throw new ArgumentException("ResourceIds is required");
            }

            string query = /*strpsql*/@"
            WITH lastChange AS (
                SELECT
                    MAX(resourceRegistryDelegationChangeId) AS changeId
                    ,R.resourceId
                    ,R.resourceRegistryId
                    ,R.resourceType
                FROM
                    accessmanagement.Resource AS R
                    INNER JOIN delegation.ResourceRegistryDelegationChanges AS DC ON DC.resourceId_fk = R.resourceid
                WHERE
                    R.resourceType = @resourceType
                    AND R.resourceRegistryId = ANY (@resourceRegistryIds)
            ";

            if (offeredByPartyId > 0)
            {
                query += /*strpsql*/@"
                    AND offeredByPartyId = @offeredByPartyId";
            }

            if (coveredByPartyId > 0)
            {
                query += /*strpsql*/@"
                    AND coveredByPartyId = @coveredByPartyId";
            }

            query += /*strpsql*/@"
                GROUP BY
                    DC.resourceId_fk
                    ,DC.offeredByPartyId
                    ,DC.coveredByPartyId
                    ,DC.coveredByUserId
                    ,R.resourceId
                    ,R.resourceRegistryId
                    ,R.resourceType
            )
            SELECT
                change.resourceRegistryDelegationChangeId
                ,change.delegationChangeType
                ,lastChange.resourceRegistryId
                ,lastChange.resourceType
                ,change.offeredByPartyId
                ,change.fromUuid
                ,change.fromType
                ,change.coveredByUserId
                ,change.coveredByPartyId
                ,change.toUuid
                ,change.toType
                ,change.performedByUserId
                ,change.performedByPartyId
                ,change.blobStoragePolicyPath
                ,change.blobStorageVersionId
                ,change.created
            FROM
                delegation.ResourceRegistryDelegationChanges AS change
                INNER JOIN lastChange ON change.resourceRegistryDelegationChangeId = lastChange.changeid
            WHERE
                delegationchangetype != 'revoke_last'
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("resourceRegistryIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceIds);
                cmd.Parameters.AddWithValue("resourceType", NpgsqlDbType.Text, resourceType.ToString().ToLower());
                cmd.Parameters.AddWithNullableValue("offeredByPartyId", NpgsqlDbType.Integer, offeredByPartyId);
                cmd.Parameters.AddWithNullableValue("coveredByPartyId", NpgsqlDbType.Integer, coveredByPartyId);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetResourceRegistryDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            const string query = /*strpsql*/@"
            WITH resources AS (
                SELECT resourceId, resourceRegistryId, resourceType
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
            fromuuid,
            fromtype,
            coveredByUserId,
            coveredByPartyId,
            touuid,
            totype,
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
            fromuuid,
            fromtype,
            coveredByUserId,
            coveredByPartyId,
            touuid,
            totype,
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
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("offeredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, offeredByPartyIds);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            if (coveredByUserIds == null && coveredByPartyIds == null)
            {
                return [];
            }

            const string query = /*strpsql*/@"
            WITH resources AS (
                SELECT resourceId, resourceRegistryId, resourceType
                FROM accessmanagement.Resource
                WHERE resourceType != 'maskinportenschema'
            ),
            latestResourceChanges AS (
                SELECT MAX(resourceRegistryDelegationChangeId) as latestId
                FROM delegation.ResourceRegistryDelegationChanges
                WHERE coveredByUserId = ANY (@coveredbyuserids) OR coveredByPartyId = ANY (@coveredByPartyIds)
                GROUP BY resourceId_fk, offeredByPartyId, coveredByUserId, coveredByPartyId
            ),
            latestAppChanges AS (
                SELECT MAX(delegationChangeId) as latestId
                FROM delegation.delegationchanges
                WHERE coveredByUserId = ANY (@coveredByUserIds) OR coveredByPartyId = ANY (@coveredByPartyIds)
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
                fromUuid,
                fromType,
                coveredByUserId,
                coveredByPartyId,
                toUuid,
                toType,
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
                fromUuid,
                fromType,
                coveredByUserId,
                coveredByPartyId,
                toUuid,
                toType,
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
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithNullableValue("coveredByUserIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByUserIds);
                cmd.Parameters.AddWithNullableValue("coveredByPartyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer, coveredByPartyIds);

                return await cmd.ExecuteEnumerableAsync(cancellationToken)
                    .SelectAwait(GetDelegationChange)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
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
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                return new DelegationChange
                {
                    DelegationChangeId = await reader.GetFieldValueAsync<int>("delegationchangeid"),
                    DelegationChangeType = await reader.GetFieldValueAsync<DelegationChangeType>(DelegationChangeType),
                    ResourceId = await reader.GetFieldValueAsync<string>("altinnappid"),
                    ResourceType = ResourceAttributeMatchType.AltinnAppId.ToString(),
                    OfferedByPartyId = await reader.GetFieldValueAsync<int>("offeredbypartyid"),
                    FromUuid = await reader.GetFieldValueAsync<Guid?>(FromUuid),
                    FromUuidType = await reader.GetFieldValueAsync<UuidType?>(FromType) ?? UuidType.NotSpecified,
                    CoveredByPartyId = await reader.GetFieldValueAsync<int?>("coveredbypartyid"),
                    CoveredByUserId = await reader.GetFieldValueAsync<int?>("coveredbyuserid"),
                    ToUuid = await reader.GetFieldValueAsync<Guid?>(ToUuid),
                    ToUuidType = await reader.GetFieldValueAsync<UuidType?>(ToType) ?? UuidType.NotSpecified,
                    PerformedByUserId = await reader.GetFieldValueAsync<int?>("performedbyuserid"),
                    BlobStoragePolicyPath = await reader.GetFieldValueAsync<string>("blobstoragepolicypath"),
                    BlobStorageVersionId = await reader.GetFieldValueAsync<string>("blobstorageversionid"),
                    Created = await reader.GetFieldValueAsync<DateTime>("created")
                };
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                return await new ValueTask<DelegationChange>(Task.FromException<DelegationChange>(ex));
            }
        }

        private static async ValueTask<DelegationChange> GetResourceRegistryDelegationChange(NpgsqlDataReader reader)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                return new DelegationChange
                {
                    ResourceRegistryDelegationChangeId = await reader.GetFieldValueAsync<int>("resourceregistrydelegationchangeid"),
                    DelegationChangeType = await reader.GetFieldValueAsync<DelegationChangeType>(DelegationChangeType),
                    ResourceId = await reader.GetFieldValueAsync<string>("resourceregistryid"),
                    ResourceType = await reader.GetFieldValueAsync<string>("resourcetype"),
                    OfferedByPartyId = await reader.GetFieldValueAsync<int>("offeredbypartyid"),
                    FromUuid = await reader.GetFieldValueAsync<Guid?>(FromUuid),
                    FromUuidType = await reader.GetFieldValueAsync<UuidType?>(FromType) ?? UuidType.NotSpecified,
                    CoveredByPartyId = await reader.GetFieldValueAsync<int?>("coveredbypartyid"),
                    CoveredByUserId = await reader.GetFieldValueAsync<int?>("coveredbyuserid"),
                    ToUuid = await reader.GetFieldValueAsync<Guid?>(ToUuid),
                    ToUuidType = await reader.GetFieldValueAsync<UuidType?>(ToType) ?? UuidType.NotSpecified,
                    PerformedByUserId = await reader.GetFieldValueAsync<int?>("performedbyuserid"),
                    PerformedByPartyId = await reader.GetFieldValueAsync<int?>("performedbypartyid"),
                    BlobStoragePolicyPath = await reader.GetFieldValueAsync<string>("blobstoragepolicypath"),
                    BlobStorageVersionId = await reader.GetFieldValueAsync<string>("blobstorageversionid"),
                    Created = await reader.GetFieldValueAsync<DateTime>("created")
                };
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                return await new ValueTask<DelegationChange>(Task.FromException<DelegationChange>(ex));
            }
        }
    }
}