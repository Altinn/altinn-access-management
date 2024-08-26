using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Seeds;
using Altinn.AccessManagement.Tests.Util;

namespace Altinn.AccessManagement.Tests.Scenarios
{
    /// <summary>
    /// Creates a bearer token that's used when calling the test server. The token will be attached to the HTTP client right after creating
    /// the client. <see cref="WebApplicationFixture.ConfigureHostBuilderWithScenarios(Scenario[])"/>
    /// </summary>
    public static class TokenScenario
    {
        /// <summary>
        /// creates a JWT user token for given person and sets the field <see cref="MockContext.HttpHeaders" />
        /// </summary>
        /// <param name="person">The person which a token should be generated</param>
        /// <param name="authenticationLevel">level of authentication [1, 2, 3] </param>
        /// <returns></returns>
        public static Scenario PersonToken(PersonSeeds.PersonBase person, int authenticationLevel = 2) => mock =>
        {
            mock.HttpHeaders.Add("Authorization", $"Bearer {PrincipalUtil.GetToken(person.UserId, person.PartyId, authenticationLevel)}");
        };

        /// <summary>
        /// Sets headers PlatformAccessToken with an access token containing given claims issuer and app
        /// </summary>
        /// <param name="issuer">issuer of the token</param>
        /// <param name="app">name of the app</param>
        /// <returns></returns>
        public static Scenario PlatformToken(string issuer, string app) => mock =>
        {
            mock.HttpHeaders.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken(issuer, app));
        };
    }
}