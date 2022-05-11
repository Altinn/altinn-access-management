using Altinn.AuthorizationAdmin.Core.Models;
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

            List<Core.Models.AuthorizationRequestResource> requestResources = new List<Core.Models.AuthorizationRequestResource>();
            requestResources.Add(new Core.Models.AuthorizationRequestResource() { ServiceCode = "asdf", ServiceEditionCode = 435 });
            delegationRequests.Add(new DelegationRequest() { CoveredBy = "test", RequestMessage = "Gi meg rettigheter", RequestResources = requestResources });

            return delegationRequests;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DelegationRequest>> Get(string id)
        {
            DelegationRequest delegationRequest = new DelegationRequest();
            delegationRequest.RequestResources = new List<Core.Models.AuthorizationRequestResource>();
            delegationRequest.RequestResources.Add(new Core.Models.AuthorizationRequestResource() { ServiceCode = "asdf", ServiceEditionCode = 435 });
            return delegationRequest;
        }
    }
}
