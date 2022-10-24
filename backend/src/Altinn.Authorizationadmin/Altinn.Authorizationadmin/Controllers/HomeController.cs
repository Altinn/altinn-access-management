using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin
{
    /// <summary>
    /// HomeController
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets the index vew for AuthorizationAdmin
        /// </summary>
        /// <returns>View result</returns>
        [HttpGet]
        [Route("accessmanagement/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            bool frontendInDevMode = AppEnvironment.GetVariable("FRONTEND_MODE") == "Development";
            string frontendDevUrl = AppEnvironment.GetVariable("FRONTEND_DEV_URL", "http://localhost:3000");

            ViewData["frontendInDevMode"] = frontendInDevMode;
            ViewData["frontendDevUrl"] = frontendDevUrl;

            if (!frontendInDevMode)
            {
                ViewData["frontendProdCss"] = _frontEndEntrypoints?.Css?[0];
                ViewData["frontendProdJs"] = _frontEndEntrypoints?.File;
            }

            return View();
        }

        private FrontEndEntryPointOptions _frontEndEntrypoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="frontEndEntrypoints">Configuration of frontend entry points</param>
        public HomeController(IOptions<FrontEndEntryPointOptions> frontEndEntrypoints)
        {
            _frontEndEntrypoints = frontEndEntrypoints.Value;
        }
    }
}
