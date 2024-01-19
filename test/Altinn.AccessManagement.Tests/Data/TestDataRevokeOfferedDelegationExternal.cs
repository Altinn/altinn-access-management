using System;
using System.Collections.Generic;
using System.Linq;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;

/// <summary>
/// testdata
/// </summary>
public static class TestDataRevokeOfferedDelegationExternal
{
    private static string PersonPaulaSSN => "02056260016";

    private static string PersonOrjanSSN => "27099450067";

    private static string ResourceOrg => "org1";

    private static string ResourceAppId => "app1";

    private static string OrganizationOrstadAccounting => "910459880";

    private static string OrganizationKolbjorn => "810419342";

    private static string EnterpriseUsername => "OrstaECUser";

    /// <summary>
    /// An input model that specifies revoking an existing delegation for resource app_org1_app1 with action read from Orjan to Paula.
    /// The delegation 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToPerson() => [[
            NewRevokeOfferedModel(
                WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute, PersonPaulaSSN),
                WithRevokeOfferedAction("read"),
                WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, ResourceOrg),
                WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, ResourceAppId)),
            IdentifierUtil.PersonHeader,
            PersonOrjanSSN,
        ]];

    /// <summary>
    /// An input model that specifies revoking an existing delegation from Paula to Orstad Accounting. The delegation should 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToOrganization() => [[
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute, OrganizationOrstadAccounting),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, ResourceOrg),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, ResourceAppId)),
        IdentifierUtil.PersonHeader,
        PersonPaulaSSN,
        ]];

    /// <summary>
    /// a
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToOrganization() => [[
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute, OrganizationKolbjorn),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, ResourceOrg),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, ResourceAppId)),
        IdentifierUtil.OrganizationNumberHeader,
        OrganizationOrstadAccounting,
        ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToPerson() => [[
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute, PersonPaulaSSN),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, ResourceOrg),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, ResourceAppId)),
        IdentifierUtil.OrganizationNumberHeader,
        OrganizationOrstadAccounting,
    ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToEnterpriseuser() => [[
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName, EnterpriseUsername),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, ResourceOrg),
            WithRevokeOfferedResource(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, ResourceAppId)),
        IdentifierUtil.OrganizationNumberHeader,
        OrganizationOrstadAccounting,
    ]];

    private static RevokeOfferedDelegationExternal NewRevokeOfferedModel(params Action<RevokeOfferedDelegationExternal>[] actions)
    {
        var model = new RevokeOfferedDelegationExternal();
        foreach (var action in actions)
        {
            action(model);
        }

        return model;
    }

    private static Action<RevokeOfferedDelegationExternal> WithRevokeOfferedAction(string action) => model =>
    {
        model.Rights ??= [new() { Action = action, Resource = [] }];
        model.Rights.First().Action = action;
    };

    private static Action<RevokeOfferedDelegationExternal> WithRevokeOfferedResource(string id, object value) => model =>
    {
        model.Rights ??= [new() { Resource = [] }];
        model.Rights.First().Resource ??= [];
        model.Rights.First().Resource.Add(new()
        {
            Id = id,
            Value = value.ToString(),
        });
    };

    private static Action<RevokeOfferedDelegationExternal> WithRevokeOfferedTo(string id, object value) => model =>
    {
        model.To ??= [];
        model.To.Add(new()
        {
            Id = id,
            Value = value.ToString(),
        });
    };
}