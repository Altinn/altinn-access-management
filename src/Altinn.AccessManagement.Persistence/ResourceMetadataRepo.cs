using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Storing Resource Rigistry metadata to Access management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceMetadataRepo : IResourceMetadataRepository
    {
        private readonly NpgsqlDataSource _conn;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepo"/> class
        /// </summary>
        /// <param name="conn">PostgreSQL datasource connection</param>
        public ResourceMetadataRepo(NpgsqlDataSource conn)
        {
            _conn = conn;
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            string query = /*strpsql*/@"
            INSERT INTO accessmanagement.resource (resourceregistryid, resourcetype, created, modified)
            VALUES (@resourceregistryid, @resourcetype, now(), now())
            ON CONFLICT (resourceregistryid) DO UPDATE SET resourcetype = @resourcetype, modified = now()
            RETURNING resourceid, resourceregistryid, resourcetype, created, modified;
            ";

            try
            {
                await using var cmd = _conn.CreateCommand(query);
                cmd.Parameters.AddWithValue("resourceregistryid", NpgsqlDbType.Text, resource.ResourceRegistryId);
                cmd.Parameters.AddWithValue("resourcetype", NpgsqlDbType.Text, resource.ResourceType.ToString().ToLower());

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return await GetAccessManagementResource(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        private static async ValueTask<AccessManagementResource> GetAccessManagementResource(NpgsqlDataReader reader)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                return new AccessManagementResource
                {
                    ResourceId = await reader.GetFieldValueAsync<int>("resourceid"),
                    ResourceRegistryId = await reader.GetFieldValueAsync<string>("resourceregistryid"),
                    ResourceType = Enum.TryParse(await reader.GetFieldValueAsync<string>("resourcetype"), out ResourceType resourceType) ? resourceType : ResourceType.Default,
                    Created = await reader.GetFieldValueAsync<DateTime>("created"),
                    Modified = await reader.GetFieldValueAsync<DateTime>("modified")
                };
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                return await new ValueTask<AccessManagementResource>(Task.FromException<AccessManagementResource>(ex));
            }
        }
    }
}
