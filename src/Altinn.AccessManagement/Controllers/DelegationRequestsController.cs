using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Resolvers;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegation requests
    /// </summary>
    [Route("api")]
    [ApiController]
    public class DelegationRequestsController : ControllerBase
    {
        private readonly IDelegationRequests _delegationRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequestsController"/> class.
        /// </summary>
        /// <param name="delegationRequsts">The service implementation for</param>
        public DelegationRequestsController(IDelegationRequests delegationRequsts)
        {
            _delegationRequests = delegationRequsts;
        }

        /// <summary>
        /// Gets a list of delegation requests for the reportee if the user is authorized
        /// </summary>
        /// <param name="who">The reportee to get delegation requests for</param>
        /// <param name="serviceCode">Optional filter parameter for serviceCode</param>
        /// <param name="serviceEditionCode">Optional filter parameter for serviceEditionCode</param>
        /// <param name="direction">Optional filter parameter for directions (incoming, outgoing). If no direction is specified, both incoming and outgoing requests will be returned</param>
        /// <param name="status">Optional filter parameter for status. (created, unopened, approved, rejected, deleted)</param>
        /// <param name="continuation">Optional filter parameter for continuationToken</param>
        /// <returns>List of delegation requests</returns>
        [HttpGet("accessmanagement/api/v1/delegationrequests/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<DelegationRequests> Get(string who, [FromQuery] string serviceCode = "", [FromQuery] int? serviceEditionCode = null, [FromQuery] RestAuthorizationRequestDirection direction = RestAuthorizationRequestDirection.Both, [FromQuery] List<RestAuthorizationRequestStatus> status = null, [FromQuery] string continuation = "")
        {
            return await _delegationRequests.GetDelegationRequestsAsync(who, serviceCode, serviceEditionCode, direction, status, continuation);
        }

        /// <summary>
        /// Gets a single Delegation Requests by its id, if the id exists and the user is authorized
        /// </summary>
        /// <param name="id">The delegation request id</param>
        /// <returns>The delegation request</returns>
        [HttpGet("accessmanagement/api/v1/delegationrequests/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<ActionResult<DelegationRequest>> Get(string id)
        {
            DelegationRequest delegationRequest = new()
            {
                RequestResources =
                [
                    new AuthorizationRequestResource() { ServiceCode = "asdf", ServiceEditionCode = 435 },
                ]
            };

            return Task.FromResult<ActionResult<DelegationRequest>>(delegationRequest);
        }
    }
}
