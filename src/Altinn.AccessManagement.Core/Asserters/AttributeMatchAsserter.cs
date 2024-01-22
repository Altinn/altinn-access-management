using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Microsoft.VisualBasic;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// asserts values for model <see cref="AttributeMatch"/>
/// </summary>
public class AttributeMatchAsserter : Asserter<AttributeMatch>
{
    /// <summary>
    /// Passes if the all the given attribute types contains in the given list of attributes.
    /// </summary>
    /// <returns></returns>
    public static Assertion<AttributeMatch> HasAttributeTypes(params string[] types) => (errors, values) =>
    {
        if (values.All(value => types.Any(type => string.Equals(type, value.Id, StringComparison.InvariantCultureIgnoreCase))))
        {
            return;
        }

        errors.Add(nameof(HasAttributeTypes), [$"the combination of {Strings.Join(values.Select(v => v.Value).ToArray(), ",")} is missing"]);
    };

    /// <summary>
    /// summary
    /// </summary>
    public static Assertion<AttributeMatch> HasOrgAndAltinnApp() => (errors, values) =>
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
    /// <param name="actions">hehe</param>
    /// <returns></returns>
    public Assertion<AttributeMatch> WithDefaultTo(params Assertion<AttributeMatch>[] actions) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            Single(
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)),
        };

        defaults.AddRange(actions);
        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };

    /// <summary>
    /// aa
    /// </summary>
    /// <param name="actions">cake</param>
    /// <returns></returns>
    public Assertion<AttributeMatch> WithDefaultFrom(params Assertion<AttributeMatch>[] actions) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            Single(
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)),
        };

        defaults.AddRange(actions);
        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };

    /// <summary>
    /// some actions
    /// </summary>
    /// <param name="actions">some action</param>
    /// <returns></returns>
    public Assertion<AttributeMatch> WithDefaultResource(params Assertion<AttributeMatch>[] actions) => (errors, values) =>
    {
        var defaults = new List<Assertion<AttributeMatch>>()
        {
            Single(
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute),
                HasAttributeTypes(AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceCodeAttribute, AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceEditionCodeAttribute)),
        };

        defaults.AddRange(actions);
        foreach (var action in defaults)
        {
            action(errors, values);
        }
    };
}