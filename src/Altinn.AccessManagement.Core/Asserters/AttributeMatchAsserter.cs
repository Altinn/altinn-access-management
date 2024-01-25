using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.VisualBasic;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// asserts values for model <see cref="AttributeMatch"/>
/// </summary>
public static class AttributeMatchAsserter
{
    /// <summary>
    /// Passes if the all the given attribute types contains in the given list of attributes.
    /// </summary>
    /// <returns></returns>
    public static Assertion<AttributeMatch> HasAttributeTypes(this IAssert<AttributeMatch> assert, params string[] attributes) => (errors, values) =>
    {
        if (values.All(value => attributes.Any(type => string.Equals(type, value.Id, StringComparison.InvariantCultureIgnoreCase))))
        {
            return;
        }

        errors.Add(nameof(HasAttributeTypes), [$"the combination of {Strings.Join(values.Select(v => v.Value).ToArray(), ",")} is missing"]);
    };

    /// <summary>
    /// summary
    /// </summary>
    public static Assertion<AttributeMatch> HasOrgAndAltinnApp(this IAssert<AttributeMatch> assert) => (errors, values) =>
    {
        if (values.Any(value => value.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute) && values.Any(value => value.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute))
        {
            return;
        }

        errors.Add(nameof(HasOrgAndAltinnApp), [$"input model is missing {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute}"]);
    };

    /// <summary>
    /// heheee
    /// </summary>
    /// <param name="assert">a</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> WithDefaultTo(this IAssert<AttributeMatch> assert) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.Single(
                assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo)),
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> WithDefaultFrom(this IAssert<AttributeMatch> assert) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.Single(
                assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo)),
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };

    /// <summary>
    /// some actions
    /// </summary>
    /// <param name="assert">a</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> WithDefaultResource(this IAssert<AttributeMatch> assert) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.Single(
                assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo, Urn.Altinn.Resource.AppId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId, Urn.Altinn.Resource.AppId),
                assert.HasAttributeTypes(Urn.Altinn.Resource.ResourceRegistryId)),
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };
}