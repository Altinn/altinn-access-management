using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Exposes API endpoints related to authentication.
    /// </summary>
    [ApiController]
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationClient _authenticationClient;
        private readonly PlatformSettings _platformSettings;
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class
        /// </summary>
        public AuthenticationController(
            IAuthenticationClient authenticationClient, 
            IOptions<GeneralSettings> settings, 
            IOptions<PlatformSettings> platformSettings,
            IOptions<GeneralSettings> generalSettings,
            ILogger<DelegationsController> logger)
        {
            _logger = logger;
            _authenticationClient = authenticationClient;
            _platformSettings = platformSettings.Value;
            _generalSettings = generalSettings.Value;
        }

        /// <summary>
        /// Refreshes the AltinnStudioRuntime JwtToken when not in AltinnStudio mode.
        /// </summary>
        /// <returns>Ok result with updated token.</returns>
        [Authorize]
        [HttpGet("accessmanagement/api/v1/authentication/refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                string token = await _authenticationClient.RefreshToken();
                CookieOptions runtimeCookieSetting = new CookieOptions
                {
                    Domain = _generalSettings.Hostname,
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };

                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpContext.Response.Cookies.Append(_platformSettings.JwtCookieName, token, runtimeCookieSetting);
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh failed to return updated token");
                return StatusCode(500);
            }
        }
    }
}
