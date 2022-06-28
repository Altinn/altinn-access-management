using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Altinn.AuthorizationAdmin.Helpers;
using Altinn.AuthorizationAdmin.Models;
using Altinn.AuthorizationAdmin.Services;

namespace Altinn.AuthorizationAdmin
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("AuthorizationAdmin/")]
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
        public HomeController(IOptions<FrontEndEntryPointOptions> frontEndEntrypoints)
        {
            _frontEndEntrypoints = frontEndEntrypoints.Value;
        }
    }
}
