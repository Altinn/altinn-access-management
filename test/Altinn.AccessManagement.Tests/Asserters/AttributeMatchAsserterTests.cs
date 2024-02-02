using System;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Altinn.AccessManagement.Tests.Asserters;

/// <summary>
/// summary
/// </summary>
public class AttributeMatchAsserterTests
{
    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultToCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultTo(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultTo);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultToCases =>
        new()
        {
            {
                [new(Urn.Altinn.Person.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Person.UserId, "123")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.PartyId, "321")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.PartyId, "<string>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(Urn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Person.IdentifierNo, string.Empty)],
                Assert.NotNull
            }
        };

    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultFromCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultFrom(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultTo);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultFromCases =>
        new()
        {
            {
                [new(Urn.Altinn.Person.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Person.UserId, "123")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.PartyId, "321")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Organization.PartyId, "<string>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(Urn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Person.IdentifierNo, string.Empty)],
                Assert.NotNull
            }
        };

    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultResourceCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultResource(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultResource);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultResourceCases =>
        new()
        {
            {
                [new(Urn.Altinn.Resource.AppOwner, "<appowner>"), new(Urn.Altinn.Resource.AppId, "<appid>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.Null
            },
            {
                [new(Urn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>"), new(Urn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(Urn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(Urn.Altinn.Resource.ResourceRegistryId, string.Empty)],
                Assert.NotNull
            }
        };
}