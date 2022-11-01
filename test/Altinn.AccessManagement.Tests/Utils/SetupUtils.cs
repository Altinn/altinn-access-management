using System;
using System.IO;
using System.Net.Http;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Tests.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Altinn.AccessManagement.Tests.Utils
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

        /// <summary>
        /// Deletes a app blob stored locally
        /// </summary>
        /// <param name="org">Org</param>
        /// <param name="app">App</param>
        public static void DeleteAppBlobData(string org, string app)
        {
            string blobPath = Path.Combine(GetDataBlobPath(), $"{org}/{app}");

            if (Directory.Exists(blobPath))
            {
                Directory.Delete(blobPath, true);
            }
        }

        private static string GetDataBlobPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "data", "blobs");
        }
    }
}
