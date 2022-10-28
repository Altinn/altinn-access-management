using Altinn.AuthorizationAdmin.Core.Clients;
using Altinn.AuthorizationAdmin.Core.Configuration;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin.Controllers
{
    /// <summary>
    /// Exposes API endpoints related to authentication.
    /// </summary>
    [Route("accessmanagement/")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationClient _authenticationClient;
        private readonly GeneralSettings _settings;
        private readonly PlatformSettings _platformSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class
        /// </summary>
        public AuthenticationController(IAuthenticationClient authenticationClient, IOptions<GeneralSettings> settings, IOptions<PlatformSettings> platformSettings)
        {
            _authenticationClient = authenticationClient;
            _settings = settings.Value;
            _platformSettings = platformSettings.Value;
        }

        /// <summary>
        /// Refreshes the AltinnStudioRuntime JwtToken when not in AltinnStudio mode.
        /// </summary>
        /// <returns>Ok result with updated token.</returns>
        [Authorize]
        [HttpGet("api/v1/authentication/refresh")]
        public async Task<IActionResult> Refresh()
        {
            string token = await _authenticationClient.RefreshToken();

            CookieOptions runtimeCookieSetting = new CookieOptions
            {
                Domain = _settings.HostName,
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                HttpContext.Response.Cookies.Append(_settings.RuntimeCookieName, token, runtimeCookieSetting);
                return Ok();
            }

            return BadRequest();
        }
    }
}
