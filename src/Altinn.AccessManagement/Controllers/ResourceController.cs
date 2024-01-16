using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller to update AccessManagement with resources existing i ResourceRegister.
    /// </summary>
    [ApiController]
    public class ResourceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IResourceAdministrationPoint _rap;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="resourceAdministrationPoint">The resource administration point</param>
        public ResourceController(
            ILogger<ResourceController> logger,
            IResourceAdministrationPoint resourceAdministrationPoint)
        {
            _logger = logger;
            _rap = resourceAdministrationPoint;
        }

        /// <summary>
        /// Updates or creates a Resource placeholder in AccessManagement to be used for describing which resource a given delegation is connected with
        /// </summary>
        /// <param name="resources">List of new Resources to add or update</param>
        /// <returns></returns>
        [HttpPost]
        [HttpPut]
        [Route("accessmanagement/api/v1/internal/resources")]
        [Authorize(Policy = "PlatformAccess")]
        [ApiExplorerSettings(IgnoreApi = true)]
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
    }
}
