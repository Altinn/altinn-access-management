using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Extensions;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Altinn.Authorization.ServiceDefaults;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.Authentication.Configuration;
using Altinn.Common.Authentication.Models;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement;

/// <summary>
/// Configures the register host.
/// </summary>
internal static class AccessManagementHost
{
    /// <summary>
    /// Configures the register host.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static WebApplication Create(string[] args)
    {
        var builder = AltinnHost.CreateWebApplicationBuilder("access-management", args);

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddControllers();
        builder.Services.AddFeatureManagement();
        builder.Services.AddHttpContextAccessor();

        builder.ConfigureAppsettings();
        builder.ConfigureInternals();
        builder.ConfigureAltinnPackages();
        builder.ConfigureOpenAPI();
        builder.ConfigureAuthorization();

        return builder.Build();
    }

    private static WebApplicationBuilder ConfigureAltinnPackages(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IPublicSigningKeyProvider, PublicSigningKeyProvider>();
        builder.Services.AddSingleton<IPDP, PDPAppSI>();
        builder.Services.AddHttpClient<AuthorizationApiClient>();
        return builder;
    }

    private static void ConfigureInternals(this WebApplicationBuilder builder)
    {
        builder.AddAccessManagementCore();
        builder.AddAccessManagementIntegration();
        builder.AddAccessManagementPersistence();
    }

    private static void ConfigureOpenAPI(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
    }

    private static void ConfigureAppsettings(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        var platformSettings = config.GetSection("PlatformSettings").Get<PlatformSettings>();
        var oidcProviders = config.GetSection("OidcProviders").Get<OidcProviderSettings>();

        builder.Services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
        builder.Services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
        builder.Services.Configure<Altinn.Common.PEP.Configuration.PlatformSettings>(config.GetSection("PlatformSettings"));
        builder.Services.Configure<CacheConfig>(config.GetSection("CacheConfig"));
        builder.Services.Configure<PostgreSQLSettings>(config.GetSection("PostgreSQLSettings"));
        builder.Services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
        builder.Services.Configure<SblBridgeSettings>(config.GetSection("SblBridgeSettings"));
        builder.Services.Configure<Altinn.Common.AccessToken.Configuration.KeyVaultSettings>(config.GetSection("kvSetting"));
        builder.Services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
        builder.Services.Configure<OidcProviderSettings>(config.GetSection("OidcProviders"));
        builder.Services.Configure<UserProfileLookupSettings>(config.GetSection("UserProfileLookupSettings"));
    }

    private static void ConfigureAuthorization(this WebApplicationBuilder builder)
    {
        PlatformSettings platformSettings = builder.Configuration.GetSection("PlatformSettings").Get<PlatformSettings>();
        OidcProviderSettings oidcProviders = builder.Configuration.GetSection("OidcProviders").Get<OidcProviderSettings>();

        builder.Services.AddSingleton<IAuthorizationHandler, AccessTokenHandler>();
        if (oidcProviders.TryGetValue("altinn", out OidcProvider altinnOidcProvder))
        {
            builder.Services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
            .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
            {
                options.JwtCookieName = platformSettings.JwtCookieName;
                options.MetadataAddress = altinnOidcProvder.Issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });
        }

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("PlatformAccess", policy => policy.Requirements.Add(new AccessTokenRequirement()));
            options.AddPolicy(AuthzConstants.ALTINNII_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "sbl.authorization")));
            options.AddPolicy(AuthzConstants.INTERNAL_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "internal.authorization")));
            options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_maskinporten_scope_delegation")));
            options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_maskinporten_scope_delegation")));
            options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATIONS_PROXY, policy => policy.Requirements.Add(new ScopeAccessRequirement(new string[] { "altinn:maskinporten/delegations", "altinn:maskinporten/delegations.admin" })));
            options.AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_access_management")));
            options.AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_access_management")));
            options.AddPolicy(AuthzConstants.POLICY_RESOURCEOWNER_AUTHORIZEDPARTIES, policy =>
                policy.Requirements.Add(new ScopeAccessRequirement([AuthzConstants.SCOPE_AUTHORIZEDPARTIES_RESOURCEOWNER, AuthzConstants.SCOPE_AUTHORIZEDPARTIES_ADMIN])));
        });

        builder.Services.AddTransient<IAuthorizationHandler, ClaimAccessHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, ResourceAccessHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, ScopeAccessHandler>();
    }
}