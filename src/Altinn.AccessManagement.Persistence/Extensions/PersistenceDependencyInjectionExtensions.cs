using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Policy;
using Azure.Core;
using Azure.Storage;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessManagement.Persistence.Extensions;

/// <summary>
/// Extension methods for adding access management services to the dependency injection container.
/// </summary>
public static class PersistenceDependencyInjectionExtensions
{
    /// <summary>
    /// Registers access management persistence services with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The <see cref="IConfiguration"/>.</param>
    /// <returns><paramref name="services"/> for further chaining.</returns>
    public static IServiceCollection AddAccessManagementPersistence(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDatabase();

        if (config.GetSection("FeatureManagement").GetValue<bool>("UseNewQueryRepo"))
        {
            services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepo>();
            services.AddSingleton<IResourceMetadataRepository, ResourceMetadataRepo>();
        }
        else
        {
            services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();
            services.AddSingleton<IResourceMetadataRepository, ResourceMetadataRepository>();
        }

        services.AdDelegationPolicyRepository();

        return services;
    }

    /// <summary>
    /// Registers a <see cref="IPolicyRepository"/> with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/> for further chaining.</returns>
    public static IServiceCollection AdDelegationPolicyRepository(
        this IServiceCollection services)
    {
        services.AddOptions<AzureStorageConfiguration>()
            .Validate(s => !string.IsNullOrEmpty(s.DelegationsAccountKey), "account key cannot be null or empty")
            .Validate(s => !string.IsNullOrEmpty(s.DelegationsAccountName), "account name cannot be null or empty")
            .Validate(s => !string.IsNullOrEmpty(s.DelegationsContainer), "container cannot be null or empty")
            .Validate(s => !string.IsNullOrEmpty(s.DelegationsBlobEndpoint), "blob endpoint cannot be null or empty");

        services.TryAddSingleton<IPolicyRepository, PolicyRepository>();

        return services;
    }

    /// <summary>
    /// Registers a <see cref="IPolicyFactory"/> with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public static IServiceCollection AddDelegationPolicyRepositoryV2(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new AzureStorageConfiguration();

        configuration
            .GetRequiredSection(nameof(AzureStorageConfiguration))
            .Bind(config);

        services.AddDelegationPolicyRepositoryV2(options =>
        {
            options.AddRange([
                new(AccountType.Delegations, config.DelegationsAccountName, config.DelegationsContainer, config.DelegationsBlobEndpoint, config.DelegationsAccountKey, config.BlobLeaseTimeout),
                new(AccountType.Metadata, config.MetadataAccountName, config.MetadataContainer, config.MetadataBlobEndpoint, config.MetadataAccountKey, config.BlobLeaseTimeout),
                new(AccountType.ResourceRegister, config.ResourceRegistryAccountName, config.ResourceRegistryContainer, config.ResourceRegistryBlobEndpoint, config.ResourceRegistryAccountKey, config.BlobLeaseTimeout),
            ]);
        });

        return services;
    }

    /// <summary>
    /// Registers a <see cref="IPolicyFactory"/> with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">options for configuring blob service</param>
    /// <returns><paramref name="services"/> for further chaining.</returns>
    public static IServiceCollection AddDelegationPolicyRepositoryV2(this IServiceCollection services, Action<List<PolicyOptions>> configureOptions)
    {
        var options = new List<PolicyOptions>();
        configureOptions(options);
        services.AddAzureClients(builder =>
        {
            foreach (var config in options)
            {
                services.AddOptions<PolicyOptions>(config.Account.ToString())
                    .Configure(policy =>
                    {
                        policy.Account = config.Account;
                        policy.AccountName = config.AccountName;
                        policy.Container = config.Container;
                        policy.Key = config.Key;
                        policy.Uri = config.Uri;
                        policy.LeaseAcquireTimeout = config.LeaseAcquireTimeout;
                    })
                    .Validate(policy => !string.IsNullOrEmpty(policy.AccountName), $"{nameof(PolicyOptions.AccountName)} cannot be null or empty for account type {config.Account}")
                    .Validate(policy => !string.IsNullOrEmpty(policy.Container), $"{nameof(PolicyOptions.Container)} cannot be null for account type {config.Account}")
                    .Validate(policy => !string.IsNullOrEmpty(policy.Key), $"{nameof(PolicyOptions.Key)} cannot be null or empty for account type {config.Account} ")
                    .Validate(policy => Uri.IsWellFormedUriString(policy.Uri, UriKind.Absolute), $"{nameof(PolicyOptions.Uri)} cannot be null for account type {config.Account}");

                builder.AddBlobServiceClient(new Uri(config.Uri), new StorageSharedKeyCredential(config.AccountName, config.Key))
                    .WithName(config.Account.ToString())
                    .ConfigureOptions(options =>
                    {
                        options.Retry.Mode = RetryMode.Exponential;
                        options.Retry.MaxRetries = 5;
                        options.Retry.MaxDelay = TimeSpan.FromSeconds(3);
                    });
            }
        });

        services.TryAddSingleton<IPolicyFactory, PolicyFactory>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddOptions<PostgreSQLSettings>()
            .Validate(s => !string.IsNullOrEmpty(s.ConnectionString), "connection string cannot be null or empty")
            .Validate(s => !string.IsNullOrEmpty(s.AuthorizationDbPwd), "connection string password be null or empty");

        services.TryAddSingleton((IServiceProvider sp) =>
        {
            var settings = sp.GetRequiredService<IOptions<PostgreSQLSettings>>().Value;

            var bld = new NpgsqlConnectionStringBuilder(string.Format(settings.ConnectionString, settings.AuthorizationDbPwd));
            bld.AutoPrepareMinUsages = 2;
            bld.MaxAutoPrepare = 50;

            var builder = new NpgsqlDataSourceBuilder(bld.ConnectionString);
            builder.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
            builder.MapEnum<DelegationChangeType>("delegation.delegationchangetype");
            builder.MapEnum<UuidType>("delegation.uuidtype");

            return builder.Build();
        });

        return services;
    }
}