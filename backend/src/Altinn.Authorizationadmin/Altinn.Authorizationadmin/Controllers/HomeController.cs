using Microsoft.AspNetCore.Mvc;

namespace Altinn.AuthorizationAdmin
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("AuthorizationAdmin/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
