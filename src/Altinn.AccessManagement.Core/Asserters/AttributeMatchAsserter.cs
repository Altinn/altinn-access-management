using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;

namespace Altinn.AccessManagement.Core.Asserters;

/// <summary>
/// Asserts values for model <see cref="AttributeMatch"/>.
/// </summary>
public static class AttributeMatchAsserter
{
    private static string StringifyAttributeIds(IEnumerable<AttributeMatch> values) => $"[{string.Join(",", values.Select(v => v.Id).OrderDescending())}]";

    private static string StringifyStrings(IEnumerable<string> values) => $"[{string.Join(",", values.OrderDescending())}]";

    /// <summary>
    /// Passes if all the given attribute types are contained in the given list of attributes.
    /// </summary>
    public static Assertion<AttributeMatch> HasAttributeTypes(this IAssert<AttributeMatch> _, params string[] attributes) => (errors, values) =>
    {
        IEnumerable<string> intersection = values.Select(v => v.Id).Intersect(attributes);
        if (intersection.Count() == attributes.Count() && intersection.Count() == values.Count())
        {
            return;
        }

        errors.Add(nameof(HasAttributeTypes), [$"attributes {StringifyAttributeIds(values)} is not a combination of {StringifyStrings(attributes)}"]);
    };

    /// <summary>
    /// Checks if all given types has a value of type integer. Attributes that don't exist in the list of attributes are ignored.
    /// </summary>
    /// <param name="assert">list of attributes</param>
    /// <param name="types">URN of the types that should be integers</param>
    public static Assertion<AttributeMatch> AttributesAreIntegers(this IAssert<AttributeMatch> assert, params string[] types) => (errors, values) =>
    {
        var matchingAttributes = values.Where(attribute => types.Any(type => type.Equals(attribute.Id, StringComparison.InvariantCultureIgnoreCase)));
        if (matchingAttributes.Where(attribute => !int.TryParse(attribute.Value, out _)) is var assertedNoneIntegers && assertedNoneIntegers.Any())
        {
            errors.Add(nameof(AttributesAreIntegers), [$"attributes {StringifyAttributeIds(values)} can't be parsed as integers"]);
        }
    };

    /// <summary>
    /// Checks if all given types has a value of type boolean. Attributes that don't exist in the list of attributes are ignored.
    /// </summary>
    /// <param name="assert">list of attributes</param>
    /// <param name="types">URN of the types that should be boolean</param>
    public static Assertion<AttributeMatch> AttributesAreBoolean(this IAssert<AttributeMatch> assert, params string[] types) => (errors, values) =>
    {
        var matchingAttributes = values.Where(attribute => types.Any(type => type.Equals(attribute.Id, StringComparison.InvariantCultureIgnoreCase)));
        if (matchingAttributes.Where(attribute => !bool.TryParse(attribute.Value, out _)) is var assertedNoneIntegers && assertedNoneIntegers.Any())
        {
            errors.Add(nameof(AttributesAreIntegers), [$"attributes {StringifyAttributeIds(values)} can't be parsed as boolean"]);
        }
    };

    /// <summary>
    /// Can pass a custom compare function that compares a single attribute an return a boolean that specifies if it passed or not.
    /// </summary>
    /// <param name="type">list of attributes</param>
    /// <param name="cmp">compare function</param>
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
    /// Passes if all attributes has a populated value field. Content is irrelevant, but it can't be an empty string or null 
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void AllAttributesHasValues(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        var attributesWithEmptyValues = values.Where(attribute => string.IsNullOrEmpty(attribute?.Value));
        if (attributesWithEmptyValues.Any())
        {
            errors.Add(nameof(AllAttributesHasValues), StringifyAttributeIds(attributesWithEmptyValues).Select(type => $"attribute {type} contains empty value").ToArray());
        }
    }

    /// <summary>
    /// Checks if a resource is delegable. The resource must be in the list of attributes otherwise it fails.
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void IsDelegatableResource(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values)
    {
        if (values.FirstOrDefault(value => value.Id.Equals(BaseUrn.Altinn.Resource.Delegable, StringComparison.InvariantCultureIgnoreCase)) is var attribute && attribute != null)
        {
            if (bool.TryParse(attribute.Value, out var value) && value)
            {
                return;
            }
            else
            {
                errors.Add(nameof(IsDelegatableResource), [$"resource is not delegable"]);
            }
        }
        else
        {
            errors.Add(nameof(IsDelegatableResource), [$"failed to find any attributes with value {BaseUrn.Altinn.Resource.Delegable}"]);
        }
    }

    /// <summary>
    /// A default list of assertions that contains the baseline for validating in input delegaton to an entity.
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void DefaultTo(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
                assert.Single(
                    assert.HasAttributeTypes(BaseUrn.Altinn.Person.IdentifierNo),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Person.Uuid),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Person.UserId),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Person.PartyId),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Organization.IdentifierNo),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Organization.Uuid),
                    assert.HasAttributeTypes(BaseUrn.Altinn.EnterpriseUser.Username),
                    assert.HasAttributeTypes(BaseUrn.Altinn.EnterpriseUser.Uuid),
                    assert.HasAttributeTypes(BaseUrn.Altinn.Organization.PartyId),
                    assert.HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute),
                    assert.HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute)),
                assert.AllAttributesHasValues,
                assert.AttributesAreIntegers(BaseUrn.InternalIds))(errors, values);

    /// <summary>
    /// A default list of assertions that contains the baseline for validating in input delegaton from an entity.
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void DefaultFrom(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
            assert.Single(
                assert.HasAttributeTypes(BaseUrn.Altinn.Person.IdentifierNo),
                assert.HasAttributeTypes(BaseUrn.Altinn.Person.Uuid),
                assert.HasAttributeTypes(BaseUrn.Altinn.Person.UserId),
                assert.HasAttributeTypes(BaseUrn.Altinn.Person.PartyId),
                assert.HasAttributeTypes(BaseUrn.Altinn.Organization.IdentifierNo),
                assert.HasAttributeTypes(BaseUrn.Altinn.Organization.Uuid),
                assert.HasAttributeTypes(BaseUrn.Altinn.Organization.PartyId),
                assert.HasAttributeTypes(BaseUrn.Altinn.EnterpriseUser.Username),
                assert.HasAttributeTypes(BaseUrn.Altinn.EnterpriseUser.Uuid),
                assert.HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)),
            assert.AllAttributesHasValues,
            assert.AttributesAreIntegers(BaseUrn.InternalIds))(errors, values);

    /// <summary>
    /// A list of assertions for validating input is a single value of either of the internal Altinn 2 identifiers: UserId or PartyId.
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void Altinn2InternalIds(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
            assert.Single(
                assert.HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute),
                assert.HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute)),
            assert.AllAttributesHasValues,
            assert.AttributesAreIntegers(BaseUrn.Altinn2InternalIds))(errors, values);

    /// <summary>
    /// A default list of assertions that contains the baseline for validating input for a resource.
    /// </summary>
    /// <param name="assert">list of assertions</param>
    /// <param name="errors">dictionary for writing assertion errors</param>
    /// <param name="values">list of attributes</param>
    public static void DefaultResource(this IAssert<AttributeMatch> assert, IDictionary<string, string[]> errors, IEnumerable<AttributeMatch> values) =>
        assert.All(
            assert.Single(
                assert.HasAttributeTypes(BaseUrn.Altinn.Resource.AppOwner, BaseUrn.Altinn.Resource.AppId),
                assert.HasAttributeTypes(BaseUrn.Altinn.Resource.ResourceRegistryId)),
            assert.AllAttributesHasValues)(errors, values);
}