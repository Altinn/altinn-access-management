﻿using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Models.Bff;
using Altinn.AccessManagement.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers.BFF
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [ApiController]
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class DelegationsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IDelegationsService _delegation;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="policyAdministrationPoint">The policy administration point</param>
        /// <param name="delegationsService">Handler for the delegation service</param>
        /// <param name="mapper">mapper handler</param>
        public DelegationsController(
            ILogger<DelegationsController> logger,
            IPolicyInformationPoint policyInformationPoint,
            IPolicyAdministrationPoint policyAdministrationPoint,
            IDelegationsService delegationsService,
            IMapper mapper)
        {
            _logger = logger;
            _pap = policyAdministrationPoint;
            _pip = policyInformationPoint;
            _delegation = delegationsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint for retrieving delegated resources between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/bff/{party}/delegations/maskinportenschema/offered")]
        public async Task<ActionResult<List<DelegationBff>>> GetOfferedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            if (string.IsNullOrEmpty(party))
            {
                return BadRequest("Missing party");
            }

            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetOfferedMaskinportenSchemaDelegations(partyMatch);
                List<DelegationBff> delegationsExternal = _mapper.Map<List<DelegationBff>>(delegations);

                return delegationsExternal;
            }
            catch (ArgumentException)
            {
                return BadRequest("Either the reportee is not found or the supplied value for who is not in a valid format");
            }
        }

        /// <summary>
        /// Endpoint for retrieving delegated resources between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/bff/{party}/delegations/maskinportenschema/received")]
        public async Task<ActionResult<List<DelegationBff>>> GetReceivedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            if (string.IsNullOrEmpty(party))
            {
                return BadRequest("Missing party");
            }

            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetReceivedMaskinportenSchemaDelegations(partyMatch);
                List<DelegationBff> delegationsExternal = _mapper.Map<List<DelegationBff>>(delegations);
                return delegationsExternal;
            }
            catch (ArgumentException)
            {
                return BadRequest("Either the reportee is not found or the supplied value for who is not in a valid format");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                _logger.LogError("Failed to fetch outbound delegations, See the error message for more details {errorMessage}", errorMessage);
                return StatusCode(500);
            }
        }
    }
}
