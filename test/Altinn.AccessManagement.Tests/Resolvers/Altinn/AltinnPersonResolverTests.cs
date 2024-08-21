using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Resolvers;
using Xunit;

namespace Altinn.AccessManagement.Tests.Resolvers.Altinn;

/// <summary>
/// Resolver tests
/// </summary>
[Collection(nameof(AttributeResolver))]
public class AltinnPersonResolverTests
{
    /// <summary>
    /// Person resolver tests
    /// </summary>
    /// <param name="attributes">attributes that are given by callee</param>
    /// <param name="wants">attributes that are wanted by callee</param>
    /// <param name="assert">assert method that verifies the result</param>
    [Theory]
    [MemberData(nameof(ResolveProfileUsingIdentifierNo), MemberType = typeof(AltinnPersonResolverTests))]
    [MemberData(nameof(ResolveProfileUsingUserId), MemberType = typeof(AltinnPersonResolverTests))]
    [MemberData(nameof(ResolveUnkownPartyIdToEmptyResult), MemberType = typeof(AltinnPersonResolverTests))]
    [MemberData(nameof(ResolveProfileUsingPartyId), MemberType = typeof(AltinnPersonResolverTests))]
    public async Task TestResolvePerson(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, Action<IEnumerable<AttributeMatch>> assert)
    {
        var resolver = ResolverServiceCollection.ConfigureServices(ResolverServiceCollection.DefaultServiceCollection);

        var result = await resolver.Resolve(attributes, wants, default);

        assert(result);
    }

    /// <summary>
    /// should resolve <see cref="AltinnPersonResolver.ResolveProfileUsingIdentifierNo"/>
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveProfileUsingIdentifierNo =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.IdentifierNo, "02056260016")],
                [BaseUrn.Altinn.Person.PartyId],
                AssertContains(new AttributeMatch(BaseUrn.Altinn.Person.UserId, "20000095"))
            }
        };

    /// <summary>
    /// should resolve <see cref="AltinnPersonResolver.ResolveProfileUsingUserId" />
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveProfileUsingUserId =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.UserId, "20000095")],
                [BaseUrn.Altinn.Person.IdentifierNo],
                AssertContains(new AttributeMatch(BaseUrn.Altinn.Person.IdentifierNo, "02056260016"))
            }
        };

    /// <summary>
    /// should resolve <see cref="AltinnPersonResolver.ResolvePartyUsingPartyId" />
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolvePartyUsingPartyId =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.UserId, "20000095")],
                [BaseUrn.Altinn.Person.IdentifierNo],
                AssertContains(new AttributeMatch(BaseUrn.Altinn.Person.IdentifierNo, "02056260016"))
            }
        };

    /// <summary>
    /// should trigger <see cref="AltinnPersonResolver.ResolveProfileUsingPartyId"/>/>
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveProfileUsingPartyId =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.PartyId, "50002203")],
                [BaseUrn.Altinn.Person.UserId],
                AssertContains(new AttributeMatch(BaseUrn.Altinn.Person.UserId, "20000095"))
            }
        };

    /// <summary>
    /// should trigger <see cref="AltinnPersonResolver.ResolvePartyUsingPartyId"/>, but return the same result as input as given
    /// party ID don't exist.
    /// </summary>
    public static TheoryData<List<AttributeMatch>, List<string>, Action<IEnumerable<AttributeMatch>>> ResolveUnkownPartyIdToEmptyResult =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.UserId, "00000000")],
                [BaseUrn.Altinn.Person.IdentifierNo],
                AssertEqual(new AttributeMatch(BaseUrn.Altinn.Person.UserId, "00000000"))
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