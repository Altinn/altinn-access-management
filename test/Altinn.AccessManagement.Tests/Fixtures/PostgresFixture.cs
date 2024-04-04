using System;
using System.Drawing.Printing;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Tests.Seeds;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// Fixture for creatinmg 
/// </summary>
public partial class PostgresFixture : IAsyncLifetime
{
    /// <summary>
    /// Test container instance
    /// </summary>
    /// <returns></returns>
    internal PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithDatabase(DbName)
        .WithCleanUp(true)
        .Build();

    private NpgsqlDataSource DataSource { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static readonly int DefaultPerformedBy = 200;

    /// <summary>
    /// table name
    /// </summary>
    public static readonly string DbName = "authorizationdb";

    /// <summary>
    /// Db user name
    /// </summary>
    public static readonly string DbUserName = "platform_authorization";

    /// <summary>
    /// Db admin name
    /// </summary>
    public static readonly string DbAdminName = "platform_authorization_admin";

    /// <summary>
    /// table name for delegation changes
    /// </summary>
    public static readonly string DbDelegationChangeTableName = "delegation.delegationchanges";

    /// <summary>
    /// table name for resources in resource registry
    /// </summary>
    public static readonly string DbDelegationResourceRegistryTableName = "delegation.resourceregistrydelegationchanges";

    /// <summary>
    /// Test container Db Passord
    /// </summary>
    public static readonly string DbPassword = "password";

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await PostgresContainer.StartAsync();

        var builder = new NpgsqlDataSourceBuilder(PostgresContainer.GetConnectionString());
        builder.MapEnum<DelegationChangeType>("delegation.delegationchangetype");
        DataSource = builder.Build();

        await CreateSystemUsersAndAssignRoles();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await PostgresContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates default DB user login settings 
    /// </summary>
    /// <returns></returns>
    public async Task CreateSystemUsersAndAssignRoles() =>
    await PostgresContainer.ExecScriptAsync(@"
            CREATE USER platform_authorization WITH PASSWORD 'Password';
            CREATE USER platform_authorization_admin WITH PASSWORD 'Password';
            ALTER ROLE platform_authorization LOGIN INHERIT;
            ALTER ROLE platform_authorization_admin LOGIN SUPERUSER INHERIT;
        ");

    /// <summary>
    /// Creates a transaction that executes a set of queueres in given order.
    /// Following queries are:
    /// - <see cref="WithInsertDelegationChange(Action{DelegationChange}[])"/>
    /// - <see cref="WithInsertDelegationChangeRR(Action{DelegationChange}[])"/>
    /// - <see cref="WithInsertResource(Action{AccessManagementResource}[])"/>
    /// </summary>
    /// <param name="queries">List of queries</param>
    public async Task SeedDatabaseTXs(params Action<NpgsqlCommand>[] queries)
    {
        var conn = await DataSource.OpenConnectionAsync();

        try
        {
            var tx = await conn.BeginTransactionAsync();

            foreach (var query in queries)
            {
                var cmd = new NpgsqlCommand(string.Empty, conn, tx);
                query(cmd);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    /// <summary>
    /// Create a delegation change in table delegation.delegationchanges
    /// </summary>
    /// <param name="modifiers">list of actions that mutates delegation changes</param>
    public static Action<NpgsqlCommand> WithInsertDelegationChange(params Action<DelegationChange>[] modifiers) => cmd =>
    {
        var delegation = new DelegationChange()
        {
            DelegationChangeType = DelegationChangeType.Grant,
        };

        foreach (var modifier in modifiers)
        {
            modifier(delegation);
        }

        cmd.CommandText = @"
        INSERT INTO delegation.delegationchanges(
            delegationchangetype,
            altinnappid,
            offeredbypartyid,
            coveredbypartyid,
            coveredbyuserid,
            performedbyuserid,
            blobstoragepolicypath,
            blobstorageversionid)
	    VALUES (
            @delegationchangetype,
            @altinnappid,
            @offeredbypartyid,
            @coveredbypartyid,
            @coveredbyuserid,
            @performedbyuserid,
            @blobstoragepolicypath,
            @blobstorageversionid);
        ";

        cmd.Parameters.AddWithValue("delegationchangetype", delegation.DelegationChangeType);
        cmd.Parameters.AddWithValue("altinnappid", delegation.ResourceId);
        cmd.Parameters.AddWithValue("offeredbypartyid", delegation.OfferedByPartyId);
        cmd.Parameters.AddWithValue("coveredbypartyid", delegation.CoveredByPartyId == null ? DBNull.Value : delegation.CoveredByPartyId);
        cmd.Parameters.AddWithValue("coveredbyuserid", delegation.CoveredByUserId == null ? DBNull.Value : delegation.CoveredByUserId);
        cmd.Parameters.AddWithValue("performedbyuserid", delegation.PerformedByUserId == null ? DefaultPerformedBy : delegation.PerformedByUserId);
        cmd.Parameters.AddWithValue("blobstoragepolicypath", delegation.BlobStoragePolicyPath ?? "/");
        cmd.Parameters.AddWithValue("blobstorageversionid", delegation.BlobStorageVersionId ?? "v1");
    };

    /// <summary>
    /// Creates a resource in table "accessmanagement.resource"
    /// </summary>
    /// <param name="modifiers">functions that modifies <see cref="AccessManagementResource"/>
    public static Action<NpgsqlCommand> WithInsertResource(params Action<AccessManagementResource>[] modifiers) => cmd =>
    {
        var resource = new AccessManagementResource();
        foreach (var modifier in modifiers)
        {
            modifier(resource);
        }

        cmd.CommandText = @"
        INSERT INTO accessmanagement.resource(
            resourceregistryid,
            resourcetype,
            created)
	    VALUES (
            @resourceregistryid,
            @resourcetype,
            @created);
        ";

        cmd.Parameters.AddWithValue("resourceregistryid", resource.ResourceRegistryId);
        cmd.Parameters.AddWithValue("resourcetype", resource.ResourceType.ToString());
        cmd.Parameters.AddWithValue("created", DateTime.UtcNow);
    };

    /// <summary>
    /// Creates a resource registry delegation change in table "delegation.resourceregistrydelegationchanges"
    /// </summary>
    /// <param name="modifiers">list of actions that mutates the delegation changes</param>
    public static Action<NpgsqlCommand> WithInsertDelegationChangeRR(params Action<DelegationChange>[] modifiers) => cmd =>
    {
        var delegation = new DelegationChange();
        foreach (var modifier in modifiers)
        {
            modifier(delegation);
        }

        cmd.CommandText = @"
        INSERT INTO delegation.resourceregistrydelegationchanges(
            delegationchangetype,
            resourceid_fk,
            offeredbypartyid,
            coveredbypartyid,
            coveredbyuserid,
            performedbyuserid,
            performedbypartyid,
            blobstoragepolicypath,
            blobstorageversionid)
        VALUES (
            @delegationchangetype
            @resourceid_fk,
            @offeredbypartyid,
            @coveredbypartyid,
            @coveredbyuserid,
            @performedbyuserid,
            @performedbypartyid,
            @blobstoragepolicypath,
            @blobstorageversionid);
        ";

        cmd.Parameters.AddWithValue("delegationchangetype", delegation.DelegationChangeType);
        cmd.Parameters.AddWithValue("resourceid_fk", delegation.ResourceId);
        cmd.Parameters.AddWithValue("offeredbypartyid", delegation.OfferedByPartyId);
        cmd.Parameters.AddWithValue("coveredbypartyid", delegation.CoveredByPartyId);
        cmd.Parameters.AddWithValue("coveredbyuserid", delegation.CoveredByUserId);
        cmd.Parameters.AddWithValue("performedbyuserid", delegation.PerformedByUserId);
        cmd.Parameters.AddWithValue("performedbypartyid", delegation.PerformedByPartyId);
        cmd.Parameters.AddWithValue("blobstoragepolicypath", delegation.BlobStoragePolicyPath);
        cmd.Parameters.AddWithValue("blobstorageversionid", delegation.BlobStorageVersionId);
    };

    /// <summary>
    /// sets the field <see cref="DelegationChange.DelegationChangeType"/> to given delegation
    /// </summary>
    /// <param name="delegation">delegation</param>
    public static void WithDelegationChangeRevokeLast(DelegationChange delegation)
    {
        delegation.DelegationChangeType = DelegationChangeType.RevokeLast;
    }

    /// <summary>
    /// Sets the field <see cref="AccessManagementResource.ResourceRegistryId"/> and <see cref="AccessManagementResource.ResourceType"/> to given resource
    /// </summary>
    /// <param name="model">resource</param>
    /// <returns></returns>
    public static Action<AccessManagementResource> WithAccessManagementResource(ServiceResource model) => resource =>
    {
        resource.ResourceRegistryId = model.Identifier;
        resource.ResourceType = model.ResourceType;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.ResourceId"/> to given "resource"
    /// </summary>
    /// <param name="resource">resource</param>
    public static Action<DelegationChange> WithResource(ServiceResource resource) => delegation =>
    {
        delegation.ResourceId = resource.Identifier;
        delegation.ResourceType = resource.ResourceType.ToString();
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="profile">user profile</param>
    /// <returns></returns>
    public static Action<DelegationChange> WithFromUser(IUserProfile profile) => delegation =>
    {
        delegation.CoveredByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "from user profile"
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given "to party"
    /// </summary>
    /// <param name="from">from user profile</param>
    /// <param name="to">to party</param>
    public static Action<DelegationChange> WithTupleUserAndParty(IUserProfile from, IParty to) => delegation =>
    {
        WithFromUser(from)(delegation);
        WithTo(to)(delegation);
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given "from party"
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given "to party"
    /// </summary>
    /// <param name="from">from party</param>
    /// <param name="to">to party</param>
    public static Action<DelegationChange> WithTupleParties(IParty from, IParty to) => delegation =>
    {
        WithFromParty(from)(delegation);
        WithTo(to)(delegation);
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public static Action<DelegationChange> WithFromParty(IParty party) => delegation =>
    {
        delegation.CoveredByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public static Action<DelegationChange> WithTo(IParty party) => delegation =>
    {
        delegation.OfferedByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByUserId"/> to given userId
    /// </summary>
    /// <param name="profile">User profile</param>
    public static Action<DelegationChange> WithPerformedByUserProfile(IUserProfile profile) => delegation =>
    {
        delegation.PerformedByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public static Action<DelegationChange> WithPerformedByParty(IParty party) => delegation =>
    {
        delegation.PerformedByPartyId = party.Party.PartyId;
    };
}