﻿using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
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
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class LookupController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRegister _register;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="mapper">mapper handler</param>
        /// <param name="register">handler for register</param>
        public LookupController(
            ILogger<DelegationsController> logger,
            IMapper mapper,
            IRegister register)
        {
            _logger = logger;
            _mapper = mapper;
            _register = register;
        }

        /// <summary>
        /// Endpoint for retrieving delegated rules between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/lookup/org/{orgNummer}")]
        public async Task<ActionResult<PartyExternal>> GetOrganisation(string orgNummer)
        {
            try
            {
                if (!IdentificatorUtil.ValidateOrganizationNumber(orgNummer))
                {
                    return BadRequest("The organisation number is not valid");
                }

                Party party = await _register.GetOrganisation(orgNummer);
                return _mapper.Map<PartyExternal>(party);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOrganisation failed to fetch organisation information");
                return StatusCode(500);
            }
        }
    }
}