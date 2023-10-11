using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [Route("accessmanagement/api/v1/policyinformation")]
    [ApiController]
    public class PolicyInformationPointController : ControllerBase
    {
        private readonly IPolicyInformationPoint _pip;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPointController"/> class.
        /// </summary>
        /// <param name="pip">The policy information point</param>
        /// <param name="mapper">The mapper</param>
        public PolicyInformationPointController(IPolicyInformationPoint pip, IMapper mapper)
        {
            _pip = pip;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <param name="delegationChangeInput">The input model that contains id info about user, reportee, resource and resourceMatchType </param>
        /// <returns>A list of delegation changes that's stored in the database </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("getdelegationchanges")]
        public async Task<ActionResult<List<DelegationChangeExternal>>> GetAllDelegationChanges([Microsoft.AspNetCore.Mvc.FromBody] DelegationChangeInput delegationChangeInput)
        {
            try
            {
                List<DelegationChange> response = await _pip.GetAllDelegations(delegationChangeInput);
                return _mapper.Map<List<DelegationChangeExternal>>(response);
            }
            catch (Exception e)
            {
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState, title: e.Message));
            }
        }
    }
}
