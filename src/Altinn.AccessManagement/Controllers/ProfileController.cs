using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// The <see cref="ProfileController"/> provides the API endpoints related to persons.
    /// </summary>
    [Route("accessmanagement/api/v1/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileClient _profileClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class
        /// </summary>
        public ProfileController(IProfileClient profileClient, IHttpContextAccessor httpContextAccessor, ILogger<ProfileController> logger)
        {
            _profileClient = profileClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Method that returns the user information about the user that is logged in
        /// </summary>
        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("user")]
        public async Task<ActionResult> GetUser()
        {
            int userId = AuthenticationHelper.GetUserId(_httpContextAccessor.HttpContext);
            if (userId == 0)
            {
                return BadRequest("The userId is not provided in the context.");
            }

            try
            {
                var user = await _profileClient.GetUserProfile(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
