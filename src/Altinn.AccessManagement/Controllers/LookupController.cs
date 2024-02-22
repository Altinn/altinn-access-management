using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;
using Altinn.Platform.Register.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for lookup
    /// </summary>
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextRetrievalService _contextRetrieval;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="mapper">mapper handler</param>
        /// <param name="contextRetrieval">handler for context retrieval</param>
        public LookupController(
            ILogger<DelegationsController> logger,
            IMapper mapper,
            IContextRetrievalService contextRetrieval)
        {
            _logger = logger;
            _mapper = mapper;
            _contextRetrieval = contextRetrieval;
        }

        /// <summary>
        /// Endpoint for retrieving party if party exists in the authenticated users reporteelist
        /// </summary>
        /// <param name="partyId">The partyId for the reportee to look up</param>
        /// <returns>Reportee if party is in authenticated users reporteelist</returns>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/lookup/reportee/{partyId}")]
        public async Task<ActionResult<PartyExternal>> GetPartyFromReporteeListIfExists(int partyId)
        {           
            try
            {
                int userId = AuthenticationHelper.GetUserId(HttpContext);
                Party party = await _contextRetrieval.GetPartyForUser(userId, partyId);

                if (party != null)
                {
                    if (party.PartyTypeName == Platform.Register.Enums.PartyType.Person)
                    {
                        party.SSN = IdentifierUtil.MaskSSN(party.SSN);
                    }
                    
                    return _mapper.Map<PartyExternal>(party);
                }
                else
                {
                    return StatusCode(404);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetReportee failed to fetch reportee information");
                return StatusCode(500);
            }
        }
    }
}
