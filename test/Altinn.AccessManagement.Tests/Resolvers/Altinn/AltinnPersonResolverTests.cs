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
public class AltinnPersonResolverTests
{
    /// <summary>
    /// summary
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="wants">b</param>
    /// <param name="assert">c</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Theory]
    [MemberData(nameof(ResolveIdentifierNoToPartyId), MemberType = typeof(AltinnPersonResolverTests))]
    [MemberData(nameof(ResolvePartyIdToIdentifierNo), MemberType = typeof(AltinnPersonResolverTests))]
    [MemberData(nameof(ResolveUnkownPartyIdToEmptyResult), MemberType = typeof(AltinnPersonResolverTests))]
    public async Task TestResolvePerson(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, Action<IEnumerable<AttributeMatch>> assert)
    {
        var resolver = ResolverServiceCollection.ConfigureServices(ResolverServiceCollection.DefaultServiceCollection);

        var result = await resolver.Resolve(attributes, wants, default);

        assert(result);
    }

    /// <summary>
    /// should resolve <see cref="AltinnPersonResolver.ResolveIdentifierNo"/>
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveIdentifierNoToPartyId =>
        new()
        {
            {
                [new(Urn.Altinn.Person.IdentifierNo, "07124912037")],
                [Urn.Altinn.Person.PartyId],
                AssertContains(new AttributeMatch(Urn.Altinn.Person.PartyId, "50002598"))
            }
        };

    /// <summary>
    /// should resolve <see cref="AltinnPersonResolver.ResolvePartyId" />
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolvePartyIdToIdentifierNo =>
        new()
        {
            {
                [new(Urn.Altinn.Person.PartyId, "50002203")],
                [Urn.Altinn.Person.IdentifierNo],
                AssertContains(new AttributeMatch(Urn.Altinn.Person.IdentifierNo, "02056260016"))
            }
        };

    /// <summary>
    /// should trigger <see cref="AltinnPersonResolver.ResolvePartyId"/>, but return the same result as input as given
    /// party ID don't exist.
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveUnkownPartyIdToEmptyResult =>
        new()
        {
            {
                [new(Urn.Altinn.Person.PartyId, "00000000")],
                [Urn.Altinn.Person.IdentifierNo],
                AssertEqual(new AttributeMatch(Urn.Altinn.Person.PartyId, "00000000"))
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