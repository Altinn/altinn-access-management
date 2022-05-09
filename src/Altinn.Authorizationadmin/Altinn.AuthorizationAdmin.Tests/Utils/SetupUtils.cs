using Altinn.Authorizationadmin.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AuthorizationAdmin.Tests.Utils
{
    public static class SetupUtil
    {
        public static HttpClient GetTestClient(
            CustomWebApplicationFactory<DelegationRequestsController> customFactory)
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
