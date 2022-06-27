using Microsoft.AspNetCore.Mvc;
using Altinn.AuthorizationAdmin.Services;

namespace Altinn.AuthorizationAdmin
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("AuthorizationAdmin/")]
        public IActionResult Index()
        {
            bool frontendInDevMode = Environment.GetEnvironmentVariable("FRONTEND_MODE") == "Development";

            ViewData["frontendInDevMode"] = frontendInDevMode;
            ViewData["frontendDevUrl"] = Environment.GetEnvironmentVariable("FRONTEND_DEV_URL") ?? "http://localhost:3000";

            if (!frontendInDevMode)
            {
                ViewData["frontendProdCss"] = _frontEndEntrypoints.GetCSSEntrypoint();
                ViewData["frontendProdJs"] = _frontEndEntrypoints.GetJSEntrypoint();
            }

            return View();
        }
        private IFrontEndEntrypoints _frontEndEntrypoints;
        public HomeController(IFrontEndEntrypoints frontEndEntrypoints)
        {
            _frontEndEntrypoints = frontEndEntrypoints;
        }
    }
}
