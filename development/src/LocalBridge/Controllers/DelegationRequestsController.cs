using Altinn.Brigde.Enums;
using Altinn.Brigde.Models;
using Altinn.Brigde.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorizationadmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelegationRequestsController : ControllerBase
    {
        private readonly IDelegationRequestsWrapper _delegationRequests;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegationRequsts"></param>
        public DelegationRequestsController(IDelegationRequestsWrapper delegationRequsts)
        {
            _delegationRequests = delegationRequsts;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<DelegationRequests> Get(string who, [FromQuery] string? serviceCode = "", [FromQuery] int? serviceEditionCode = null, [FromQuery] RestAuthorizationRequestDirection direction = RestAuthorizationRequestDirection.Both, [FromQuery] List<RestAuthorizationRequestStatus>? status = null, [FromQuery] string? continuation = "")
        {
           return await _delegationRequests.GetDelegationRequestsAsync(who, serviceCode, serviceEditionCode, direction, status, continuation);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DelegationRequest>> Get(string id)
        {
            DelegationRequest delegationRequest = new DelegationRequest();
            return delegationRequest;
        }
    }
}
