using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorizationadmin.Controllers
{
    [Route("api")]
    [ApiController]
    public class DelegationRequestsController : ControllerBase
    {
        private readonly IDelegationRequests _delegationRequests;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegationRequsts"></param>
        public DelegationRequestsController(IDelegationRequests delegationRequsts)
        {
            _delegationRequests = delegationRequsts;
        }


        [HttpGet("{who}/[controller]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<DelegationRequests> Get(string who, [FromQuery]  string? serviceCode = "", [FromQuery]  int? serviceEditionCode = null, [FromQuery]  RestAuthorizationRequestDirection direction = RestAuthorizationRequestDirection.Both, [FromQuery] List<RestAuthorizationRequestStatus>? status = null, [FromQuery] string? continuation = "")
        {
           return await _delegationRequests.GetDelegationRequestsAsync(who, serviceCode, serviceEditionCode, direction, status, continuation);
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
