using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Models;
using Altinn.Platform.Register.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for resource register
    /// </summary>
    [Route("accessmanagement/api")]
    [ApiController]
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class ResourceRegistryController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IResourceAdministrationPoint _resourceAdministrationPoint;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        ///<param name="resourceRegistryClient">the resource register handler</param>
        /// <param name="mapper">mapper handler</param>
        public ResourceRegistryController(
            ILogger<ResourceRegistryController> logger,
            IResourceAdministrationPoint resourceAdministrationPoint,
            IMapper mapper)
        {
            _logger = logger;
            _resourceAdministrationPoint = resourceAdministrationPoint;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of maskinprotenschema resources
        /// </summary>
        /// <param name="party">the partyid</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("v1/{party}/resources/maskinportenschema")]
        public async Task<ActionResult<List<ServiceResourceExternal>>> Get([FromRoute] int party) 
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            if (party == 0)
            {
                return BadRequest("Missing party");
            }

            resources = await _resourceAdministrationPoint.GetResources(ResourceType.MaskinportenSchema);
            return _mapper.Map<List<ServiceResourceExternal>>(resources);
        }

    }
}
