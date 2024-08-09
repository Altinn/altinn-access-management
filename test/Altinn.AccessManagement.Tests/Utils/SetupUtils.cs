using System;
using System.IO;
using System.Net.Http;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Controllers;
using Altinn.AccessManagement.Tests.Mocks;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                    services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
                    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueueMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAuthenticationClient, AuthenticationMock>();
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

        /// <summary>
        /// Adds an auth cookie to the request message
        /// </summary>
        /// <param name="requestMessage">the request message</param>
        /// <param name="token">the tijen to be added in the cookie</param>
        /// <param name="cookieName">the name of the cookie</param>
        /// <param name="xsrfToken">the xsrf token</param>
        public static void AddAuthCookie(HttpRequestMessage requestMessage, string token, string cookieName, string xsrfToken = null)
        {
            requestMessage.Headers.Add("Cookie", cookieName + "=" + token);
            if (xsrfToken != null)
            {
                requestMessage.Headers.Add("X-XSRF-TOKEN", xsrfToken);
            }
        }

        private static string GetDataBlobPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "data", "blobs");
        }
    }
}
