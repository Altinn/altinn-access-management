using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Seeds;
using Altinn.AccessManagement.Tests.Util;

namespace Altinn.AccessManagement.Tests.Scenarios
{
    /// <summary>
    /// Creates a bearer token that's used when calling the test server. The token will be attached to the HTTP client right after creating
    /// the client. <see cref="WebApplicationFixture.UseScenarios(Scenario[])"/>
    /// </summary>
    public static class TokenScenario
    {
        /// <summary>
        /// creates a JWT user token for given person and sets the field <see cref="MockContext.JwtToken" />
        /// </summary>
        /// <param name="person">The person which a token should be generated</param>
        /// <param name="authenticationLevel">level of authentication [1, 2, 3] </param>
        /// <returns></returns>
        public static Scenario PersonToken(PersonSeeds.PersonBase person, int authenticationLevel = 2) => (host, postgres, mock) =>
        {
            mock.JwtToken = PrincipalUtil.GetToken(person.UserId, person.PartyId, authenticationLevel);
        };
    }
}