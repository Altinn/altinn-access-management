using System.Net.Http;
using Altinn.Authorizationadmin.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Altinn.AuthorizationAdmin.Tests.Utils
{
    /// <summary>
    /// Utility class for usefull common operations for setup for unittests
    /// </summary>
    public static class SetupUtils
    {
        /// <summary>
        /// Gets a HttpClient for unittests testing
        /// </summary>
        /// <param name="customFactory">Web app factory to configure test services for</param>
        /// <returns>HttpClient</returns>
        public static HttpClient GetTestClient(CustomWebApplicationFactory<DelegationRequestsController> customFactory)
        {
            WebApplicationFactory<DelegationRequestsController> factory = customFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                });
            });
            factory.Server.AllowSynchronousIO = true;
            return factory.CreateClient();
        }
    }
}
