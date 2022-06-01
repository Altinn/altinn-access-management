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
        public async Task<DelegationRequests> Get()
        {
           return await _delegationRequests.GetDelegationRequestsAsync(0, 0, null);
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
