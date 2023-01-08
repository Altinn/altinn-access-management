using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller to update AccessManagement with resources existing i ResourceRegister.
    /// </summary>
    [ApiController]
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class ResourceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IResourceAdministrationPoint _rap;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="resourceAdministrationPoint">The resource administration point</param>
        /// <param name="mapper">mapper handler</param>
        public ResourceController(
            ILogger<ResourceController> logger,
            IResourceAdministrationPoint resourceAdministrationPoint,
            IMapper mapper)
        {
            _logger = logger;
            _rap = resourceAdministrationPoint;
            _mapper = mapper;
        }

        /// <summary>
        /// Updates or creates a Resource placeholder in AccessManagement to be used for describing which resource a given delegation is connected with
        /// </summary>
        /// <param name="resources">List of new Resources to add or update</param>
        /// <returns></returns>
        [HttpPost]
        [HttpPut]
        [Route("accessmanagement/api/v1/internal/resources")]
        public async Task<ActionResult> Post([FromBody] List<AccessManagementResource> resources)
        {
            if (resources.Count < 1)
            {
                return BadRequest("Missing resources in body");
            }

            List<AccessManagementResource> addResourceResult = await _rap.TryWriteResourceFromResourceRegister(resources);

            if (addResourceResult.Count == resources.Count)
            {
                return Created("Created", addResourceResult);
            }

            if (addResourceResult.Count > 0)
            {
                return StatusCode(206, addResourceResult);
            }

            string resourcesJson = JsonSerializer.Serialize(resources);
            _logger.LogInformation("Delegation could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{resourcesJson}", resourcesJson);
            return BadRequest("Delegation could not be completed");
        }

        /// <summary>
        /// Get list of maskinprotenschema resources
        /// </summary>
        /// <param name="party">the partyid</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/{party}/resources/maskinportenschema")]
        public async Task<ActionResult<List<ServiceResourceExternal>>> Get([FromRoute] int party)
        {
            List<ServiceResource> resources = new List<ServiceResource>();

            resources = await _rap.GetResources(ResourceType.MaskinportenSchema);
            return _mapper.Map<List<ServiceResourceExternal>>(resources);
        }
    }
}
