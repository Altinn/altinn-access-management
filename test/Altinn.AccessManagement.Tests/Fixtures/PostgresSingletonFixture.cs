using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Tests.Seeds;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// 
/// </summary>
public static class PostgresFixture
{
    private static PostgreSqlContainer PostgresServer { get; } = new PostgreSqlBuilder()
        .WithCleanUp(true)
        .WithStartupCallback(static async (container, cancellationToken) =>
        {
            await container.ExecAsync(
                [$@"
                CREATE USER {DbUserName} WITH PASSWORD '{DbPassword}';
                CREATE USER {DbAdminName} WITH PASSWORD '{DbPassword}';
                ALTER ROLE {DbUserName} LOGIN INHERIT;
                ALTER ROLE {DbAdminName} LOGIN SUPERUSER INHERIT;"],
                cancellationToken);
        })
        .Build();

    private static readonly string DbPassword = "Password";

    private static readonly string DbUserName = "platform_authorization";

    private static readonly string DbAdminName = "platform_authorization_admin";

    private static readonly int MaxDbs = 20;

    private static readonly SemaphoreSlim Pool = new(MaxDbs, MaxDbs);

    /// <summary>
    /// Creates a new database and connection string
    /// </summary>
    public static async Task<PostgresDb> NewDb()
    {
        await Pool.WaitAsync();

        var dbname = $"test_{Guid.NewGuid()}";
        var result = await PostgresServer.ExecScriptAsync($"CREATE DATABASE {dbname}");
        var admin = new NpgsqlConnectionStringBuilder(PostgresServer.GetConnectionString())
        {
            Username = DbAdminName,
            Password = DbPassword,
            Database = dbname,
        };

        var api = new NpgsqlConnectionStringBuilder(PostgresServer.GetConnectionString())
        {
            Username = DbUserName,
            Password = DbPassword,
            Database = dbname,
        };

        return new PostgresDb(dbname, admin, api);
    }

    /// <summary>
    /// Destroys given Database
    /// </summary>
    public static async Task DestroyDb(PostgresDb db)
    {
        var result = await PostgresServer.ExecScriptAsync($"DROP DATABASE {db.Dbname};");
        if (result.ExitCode == 0)
        {
            Pool.Release();
        }
    }
}

/// <summary>
/// Container for persisting connections string and database name
/// </summary>
public class PostgresDb : IAsyncDisposable
{
    /// <summary>
    /// ctor
    /// </summary>
    public PostgresDb(string dbname, NpgsqlConnectionStringBuilder admin, NpgsqlConnectionStringBuilder user)
    {
        Dbname = dbname;
        Admin = admin;
        User = user;
    }

    public string Dbname { get; }

    public NpgsqlConnectionStringBuilder Admin { get; }

    public NpgsqlConnectionStringBuilder User { get; }

    public async ValueTask DisposeAsync()
    {
        await PostgresFixture.DestroyDb(this);
    }
}

public class RepositoryContainer(IDelegationMetadataRepository delegationMetadataRepository, IResourceMetadataRepository resourceMetadataRepository)
{
    public IDelegationMetadataRepository DelegationMetadataRepository { get; } = delegationMetadataRepository;

    public IResourceMetadataRepository ResourceMetadataRepository { get; } = resourceMetadataRepository;

    public DelegationChange NewDelegationChange(params Action<DelegationChange>[] actions)
    {
        var delegation = new DelegationChange()
        {
            DelegationChangeType = DelegationChangeType.Grant
        };

        foreach (var action in actions)
        {
            action(delegation);
        }

        return delegation;
    }

    /// <summary>
    /// sets the field <see cref="DelegationChange.DelegationChangeType"/> to given delegation
    /// </summary>
    /// <param name="delegation">delegation</param>
    public void WithDelegationChangeRevokeLast(DelegationChange delegation)
    {
        delegation.DelegationChangeType = DelegationChangeType.RevokeLast;
    }

    /// <summary>
    /// Sets the field <see cref="AccessManagementResource.ResourceRegistryId"/> and <see cref="AccessManagementResource.ResourceType"/> to given resource
    /// </summary>
    /// <param name="model">resource</param>
    /// <returns></returns>
    public Action<AccessManagementResource> WithAccessManagementResource(ServiceResource model) => resource =>
    {
        resource.ResourceRegistryId = model.Identifier;
        resource.ResourceType = model.ResourceType;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.ResourceId"/> to given "resource"
    /// </summary>
    /// <param name="resource">resource</param>
    public Action<DelegationChange> WithResource(IAccessManagementResource resource) => delegation =>
    {
        delegation.ResourceId = resource.Resource.Identifier;
        delegation.ResourceType = resource.Resource.ResourceType.ToString();
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="profile">user profile</param>
    /// <returns></returns>
    public Action<DelegationChange> WithToUser(IUserProfile profile) => delegation =>
    {
        delegation.CoveredByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public Action<DelegationChange> WithToParty(IParty party) => delegation =>
    {
        delegation.CoveredByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public Action<DelegationChange> WithFrom(IParty party) => delegation =>
    {
        delegation.OfferedByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByUserId"/> to given userId
    /// </summary>
    /// <param name="profile">User profile</param>
    public Action<DelegationChange> WithPerformedByUserProfile(IUserProfile profile) => delegation =>
    {
        delegation.PerformedByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public Action<DelegationChange> WithPerformedByParty(IParty party) => delegation =>
    {
        delegation.PerformedByPartyId = party.Party.PartyId;
    };
}