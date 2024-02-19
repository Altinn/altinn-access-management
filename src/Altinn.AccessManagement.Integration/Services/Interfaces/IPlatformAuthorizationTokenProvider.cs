namespace Altinn.AccessManagement.Integration.Services.Interfaces;

/// <summary>
/// Provides a platform authorization access token that can be used by HTTP clients for authorization for SBL Bridge
/// </summary>
public interface IPlatformAuthorizationTokenProvider
{
    /// <summary>
    /// Gets the platform authorization token.
    /// </summary>
    /// <returns>An platform authorization token as a printable string</returns>
    public Task<string> GetAccessToken();
}
