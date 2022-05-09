using Altinn.AuthorizationAdmin.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorizationadmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelegationRequestsController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DelegationRequests>> Get()
        {
            DelegationRequests delegationRequests = new DelegationRequests();
            delegationRequests.Add(new DelegationRequest() { CoveredBy = "test" });

            return delegationRequests;
        }
    }
}
