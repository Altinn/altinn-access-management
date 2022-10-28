using System.Web;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.AuthorizationAdmin.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin
{
    /// <summary>
    /// HomeController
    /// </summary>
    [Route("accessmanagement/")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly IAntiforgery _antiforgery;
        private readonly PlatformSettings _platformSettings;
        private readonly IWebHostEnvironment _env;
        private FrontEndEntryPointOptions _frontEndEntrypoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="frontEndEntrypoints">Configuration of frontend entry points</param>
        /// <param name="antiforgery">the anti forgery service</param>
        /// <param name="platformSettings">settings related to the platform</param>
        /// <param name="env">the current environment</param>
        public HomeController(
            IOptions<FrontEndEntryPointOptions> frontEndEntrypoints, 
            IAntiforgery antiforgery,
            IOptions<PlatformSettings> platformSettings,
            IWebHostEnvironment env)
        {
            _frontEndEntrypoints = frontEndEntrypoints.Value;
            _antiforgery = antiforgery;
            _platformSettings = platformSettings.Value;
            _env = env;
        }

        /// <summary>
        /// Gets the index vew for Accessmanagement
        /// </summary>
        /// <returns>View result</returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            // See comments in the configuration of Antiforgery in MvcConfiguration.cs.
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
            {
                HttpOnly = false // Make this cookie readable by Javascript.
            });

            if (ShouldShowAppView())
            {
                return View();
            }

            string scheme = _env.IsDevelopment() ? "http" : "https";
            string goToUrl = HttpUtility.UrlEncode($"{scheme}://{Request.Host}/accessmanagement");

            string redirectUrl = $"{_platformSettings.ApiAuthenticationEndpoint}authentication?goto={goToUrl}";

            return Redirect(redirectUrl);
        }

        private bool ShouldShowAppView()
        {
            if (User.Identity.IsAuthenticated)
            {
                return true;
            }

            return false;
        }
    }
}
