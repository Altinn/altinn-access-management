using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.VisualBasic;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// asserts values for model <see cref="AttributeMatch"/>
/// </summary>
public static class AttributeMatchAsserter
{
    private static string StringifyAttributeIds(IEnumerable<AttributeMatch> values) => $"[{Strings.Join(values.Select(v => v.Id).ToArray(), ",")}]";

    /// <summary>
    /// Passes if the all the given attribute types contains in the given list of attributes.
    /// </summary>
    /// <returns></returns>
    public static Assertion<AttributeMatch> HasAttributeTypes(this IAssert<AttributeMatch> _, params string[] attributes) => (errors, values) =>
    {
        if (!values.All(value => attributes.Any(type => string.Equals(type, value.Id, StringComparison.InvariantCultureIgnoreCase))))
        {
            errors.Add(nameof(HasAttributeTypes), [$"attributes {StringifyAttributeIds(values)} is not configured as an interpretable combination"]);
        }
    };

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="types">b</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> AttributesAreIntegers(this IAssert<AttributeMatch> assert, params string[] types) => (errors, values) =>
    {
        var matchingAttributes = values.Where(attribute => types.Any(type => type.Equals(attribute.Id, StringComparison.InvariantCultureIgnoreCase)));
        var assertedNoneIntegers = matchingAttributes.Where(attribute => !int.TryParse(attribute.Value, out _));
        if (assertedNoneIntegers.Any())
        {
            errors.Add(nameof(AttributesAreIntegers), [$"attributes {StringifyAttributeIds(values)} can't be parsed as integers"]);
        }
    };

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="types">b</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> AttributesAreBoolean(this IAssert<AttributeMatch> assert, params string[] types) => (errors, values) =>
    {
        var matchingAttributes = values.Where(attribute => types.Any(type => type.Equals(attribute.Value, StringComparison.InvariantCultureIgnoreCase)));
        if (matchingAttributes.Where(attribute => !bool.TryParse(attribute.Value, out _)) is var assertedNoneIntegers && assertedNoneIntegers.Any())
        {
            errors.Add(nameof(AttributesAreIntegers), [$"attributes {StringifyAttributeIds(values)} can't be parsed as boolean"]);
        }
    };

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="type">a</param>
    /// <param name="cmp">b</param>
    /// <returns></returns>
    public static Assertion<AttributeMatch> AttributeCompare(string type, Func<AttributeMatch, bool> cmp) => (errors, values) =>
    {
        if (values.FirstOrDefault(value => value.Id.Equals(type, StringComparison.InvariantCultureIgnoreCase)) is var value && value != null)
        {
            if (cmp(value))
            {
                return;
            }

            errors.Add(nameof(AttributeCompare), [$"comparable functions for attribute {type} gave inequal result"]);
        }
        else
        {
            errors.Add(nameof(AttributeCompare), [$"could not find an attribute with type {StringifyAttributeIds(values)}"]);
        }
    };

    /// <summary>
    /// Passes if all attributes has a populated value field
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void AllAttributesHasValues(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        var attributesWithEmptyValues = values.Where(attribute => string.IsNullOrEmpty(attribute?.Value));
        if (attributesWithEmptyValues.Any())
        {
            errors.Add(nameof(AllAttributesHasValues), StringifyAttributeIds(attributesWithEmptyValues).Select(type => $"attribute {type} contains empty value").ToArray());
        }
    }

    /// <summary>
    /// summmary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void IsDelegatableResource(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        if (values.FirstOrDefault(value => value.Id.Equals(Urn.Altinn.Resource.Delegable, StringComparison.InvariantCultureIgnoreCase)) is var attribute && attribute != null)
        {
            if (bool.TryParse(attribute.Value, out var value) && value)
            {
                return;
            }
        }

        errors.Add(nameof(IsDelegatableResource), ["resource is not delegable"]);
    }

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultTo(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
                assert.Single(
                    assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                    assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo),
                    assert.HasAttributeTypes(Urn.Altinn.EnterpriseUser.Username),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId)),
                assert.AllAttributesHasValues,
                assert.AttributesAreIntegers([.. Urn.PartyIds]))(errors, values);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultFrom(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
            assert.Single(
                assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo),
                assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId)),
            assert.AllAttributesHasValues,
            assert.AttributesAreIntegers([.. Urn.PartyIds]))(errors, values);

    /// <summary>
    /// some actions
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultResource(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
            assert.Single(
                assert.HasAttributeTypes(Urn.Altinn.Resource.AppOwner, Urn.Altinn.Resource.AppId),
                assert.HasAttributeTypes(Urn.Altinn.Resource.ResourceRegistryId)),
            assert.AllAttributesHasValues)(errors, values);
}