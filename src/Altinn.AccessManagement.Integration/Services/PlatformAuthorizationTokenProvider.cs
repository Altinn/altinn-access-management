using System.Security.Cryptography.X509Certificates;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.Authentication.Configuration;
using Microsoft.Extensions.Options;
using AccessTokenSettings = Altinn.Common.AccessTokenClient.Configuration.AccessTokenSettings;

namespace Altinn.AccessManagement.Integration.Services;

/// <inheritdoc />
public class PlatformAuthorizationTokenProvider : IPlatformAuthorizationTokenProvider
{
    private readonly IKeyVaultService _keyVaultService;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly OidcProviderSettings _oidcProviderSettings;
    private readonly AccessTokenSettings _accessTokenSettings;
    private readonly KeyVaultSettings _keyVaultSettings;
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    private DateTime _cacheTokenUntil = DateTime.MinValue;
    private string _accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAuthorizationTokenProvider"/> class.
    /// </summary>
    /// <param name="keyVaultService">The key vault service.</param>
    /// <param name="accessTokenGenerator">The access token generator.</param>
    /// <param name="accessTokenSettings">The access token settings.</param>
    /// <param name="keyVaultSettings">The key vault settings.</param>
    /// <param name="oidcProviderSettings">The oidc provider settings.</param>
    public PlatformAuthorizationTokenProvider(
        IKeyVaultService keyVaultService,
        IAccessTokenGenerator accessTokenGenerator,
        IOptions<AccessTokenSettings> accessTokenSettings,
        IOptions<KeyVaultSettings> keyVaultSettings,
        IOptions<OidcProviderSettings> oidcProviderSettings)
    {
        _keyVaultService = keyVaultService;
        _accessTokenGenerator = accessTokenGenerator;
        _accessTokenSettings = accessTokenSettings.Value;
        _keyVaultSettings = keyVaultSettings.Value;
        _oidcProviderSettings = oidcProviderSettings.Value;
    }

    /// <inheritdoc />
    public async Task<string> GetAccessToken()
    {
        await Semaphore.WaitAsync();

        try
        {
            if (_accessToken == null || _cacheTokenUntil < DateTime.UtcNow)
            {
                string certBase64 = await _keyVaultService.GetCertificateAsync(_keyVaultSettings.SecretUri, "JWTCertificate");
                _accessToken = _accessTokenGenerator.GenerateAccessToken(
                    _oidcProviderSettings["altinn"].Issuer,
                    "platform.authorization",
                    new X509Certificate2(Convert.FromBase64String(certBase64), (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable));

                _cacheTokenUntil = DateTime.UtcNow.AddSeconds(_accessTokenSettings.TokenLifetimeInSeconds - 2); // Add some slack to avoid tokens expiring in transit
            }

            return _accessToken;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
