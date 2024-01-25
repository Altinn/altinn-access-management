using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.VisualBasic;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// asserts values for model <see cref="AttributeMatch"/>
/// </summary>
public static class AttributeMatchAsserter
{
    private static string StringifyAttributeIds(IEnumerable<AttributeMatch> values) => $"{Strings.Join(values.Select(v => v.Id).ToArray(), ",")}";

    private static string StringifyAttributeValues(IEnumerable<AttributeMatch> values) => $"{Strings.Join(values.Select(v => v.Id).ToArray(), ",")}";

    /// <summary>
    /// Passes if the all the given attribute types contains in the given list of attributes.
    /// </summary>
    /// <returns></returns>
    public static Assertion<AttributeMatch> HasAttributeTypes(this IAssert<AttributeMatch> _, params string[] attributes) => (errors, values) =>
    {
        if (values.All(value => attributes.Any(type => string.Equals(type, value.Id, StringComparison.InvariantCultureIgnoreCase))))
        {
            return;
        }

        errors.Add(nameof(HasAttributeTypes), [$"attributes {StringifyAttributeIds(values)} is not configured as a interpretable combination"]);
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
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultTo(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.All(
                assert.Single(
                    assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                    assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo)),
                assert.AllAttributesHasValues),
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    }

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultFrom(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.All(
                assert.Single(
                    assert.HasAttributeTypes(Urn.Altinn.Person.IdentifierNo),
                    assert.HasAttributeTypes(Urn.Altinn.Person.PartyId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo)),
                assert.AllAttributesHasValues)
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    }

    /// <summary>
    /// some actions
    /// </summary>
    /// <param name="assert">a</param>
    /// <param name="errors">b</param>
    /// <param name="values">c</param>
    public static void DefaultResource(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            assert.All(
                assert.Single(
                    assert.HasAttributeTypes(Urn.Altinn.Organization.IdentifierNo, Urn.Altinn.Resource.AppId),
                    assert.HasAttributeTypes(Urn.Altinn.Organization.PartyId, Urn.Altinn.Resource.AppId),
                    assert.HasAttributeTypes(Urn.Altinn.Resource.ResourceRegistryId)),
                assert.AllAttributesHasValues)
        };

        foreach (var action in defaults)
        {
            action(errors, values);
        }
    }
}