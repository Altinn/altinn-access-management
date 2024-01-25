using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Resolvers;
using Xunit;

namespace Altinn.AccessManagement.Tests.Resolvers.Altinn;

/// <summary>
/// summary
/// </summary>
[Collection(nameof(AttributeResolver))]
public class AltinnOrganizationResolverTests
{
    /// <summary>
    /// summary
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="wants">b</param>
    /// <param name="assert">c</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Theory]
    [MemberData(nameof(ResolveIdentifierNoToPartyId), MemberType = typeof(AltinnOrganizationResolverTests))]
    [MemberData(nameof(ResolvePartyIdToIdentifierNo), MemberType = typeof(AltinnOrganizationResolverTests))]
    [MemberData(nameof(ResolveUnkownPartyIdToEmptyResult), MemberType = typeof(AltinnOrganizationResolverTests))]
    public async Task TestResolveOrganzation(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, Action<IEnumerable<AttributeMatch>> assert)
    {
        var resolver = ResolverServiceCollection.ConfigureServices(ResolverServiceCollection.DefaultServiceCollection);

        var result = await resolver.Resolve(attributes, wants, default);

        assert(result);
    }

    /// <summary>
    /// should resolve <see cref="AltinnOrganizationResolver.ResolveOrganizationNumber"/>
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveIdentifierNoToPartyId =>
        new()
        {
            {
                [new(Urn.Altinn.Organization.IdentifierNo, "910493353")],
                [Urn.Altinn.Organization.PartyId],
                AssertContains(new AttributeMatch(Urn.Altinn.Organization.PartyId, "50006078"))
            }
        };

    /// <summary>
    /// should resolve <see cref="AltinnOrganizationResolver.ResolvePartyId"/>
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolvePartyIdToIdentifierNo =>
        new()
        {
            {
                [new(Urn.Altinn.Organization.PartyId, "50006078")],
                [Urn.Altinn.Organization.IdentifierNo],
                AssertContains(new AttributeMatch(Urn.Altinn.Organization.IdentifierNo, "910493353"))
            }
        };

    /// <summary>
    /// should trigger <see cref="AltinnOrganizationResolver.ResolvePartyId"/>, but return the same result as input as given
    /// party ID don't exist.
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveUnkownPartyIdToEmptyResult =>
        new()
        {
            {
                [new(Urn.Altinn.Organization.PartyId, "00000000")],
                [Urn.Altinn.Organization.IdentifierNo],
                AssertContains(new AttributeMatch(Urn.Altinn.Organization.PartyId, "00000000"))
            }
        };

    private static Action<IEnumerable<AttributeMatch>> AssertEqual(params AttributeMatch[] attributes) => result =>
    {
        AssertContains(attributes)(result);
        AssertContains([.. result])(attributes);
    };

    private static Action<IEnumerable<AttributeMatch>> AssertContains(params AttributeMatch[] attributes) => result =>
    {
        foreach (var attribute in attributes)
        {
            Assert.Contains(attribute, result);
        }
    };
}