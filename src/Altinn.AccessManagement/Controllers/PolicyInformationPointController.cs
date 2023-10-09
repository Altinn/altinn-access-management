using System.Text;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
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
    [ApiController]
    [Route("accessmanagement/api/v1/policyinformation")]
    public class PolicyInformationPointController : ControllerBase
    {
        private readonly IPolicyInformationPoint _pip;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPointController"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
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
        /// [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost]
        [Route("getdelegationchanges")]
        public async Task<ActionResult<List<DelegationChange>>> GetAllDelegationChanges([FromBody] DelegationChangeInput delegationChangeInput)
        {
            bool validUser = DelegationHelper.TryGetUserIdFromAttributeMatch(delegationChangeInput.Subject.SingleToList(), out int userId);
            bool validParty = DelegationHelper.TryGetPartyIdFromAttributeMatch(delegationChangeInput.Party.SingleToList(), out int partyId);
            bool validResourceMatchType = DelegationHelper.TryGetResourceFromAttributeMatch(delegationChangeInput.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string _, out string _, out string _, out string _);
            
            return await _pip.GetAllDelegations(userId, partyId, resourceId, resourceMatchType);
        }
    }
}
