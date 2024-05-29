using System;
using System.Collections.Generic;
using System.Linq;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Util;

/// <summary>
/// testdata
/// </summary>
public static class TestDataRevokeOfferedDelegationExternal
{
    private static string PersonPaulaSSN => "02056260016";

    private static int PersonPaulaUserId => 20000095;

    private static int PersonPaulaPartyId => 50002203;

    private static string PersonOrjanSSN => "27099450067";

    private static int PersonOrjanUserId => 20001337;

    private static int PersonOrjanPartyId => 50003899;

    private static string ResourceOrg => "org1";

    private static string ResourceAppId => "app1";

    private static string OrganizationOrstaAccounting => "910459880";

    private static int OrganizationOrstaPartyId => 50005545;

    private static string PersonKasperSSN => "07124912037";

    private static int PersonKasperUserId => 20000490;

    private static int PersonKasperPartyId => 50002598;

    private static string OrganizationKolbjorn => "810419342";

    private static int OrganizationKolbjornPartyId => 50004226;

    private static string EnterpriseUsername => "OrstaECUser";

    /// <summary>
    /// An input model that specifies revoking an existing delegation for resource app_org1_app1 with action read from Orjan to Paula.
    /// The delegation 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToPerson() => [[
        PrincipalUtil.GetToken(PersonOrjanUserId, PersonOrjanPartyId, 3),
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, PersonPaulaUserId),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        PersonOrjanPartyId
    ]];

    /// <summary>
    /// An input model that specifies revoking an existing delegation from Paula to Orstad Accounting. The delegation should 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToOrganization() => [[
        PrincipalUtil.GetToken(PersonPaulaUserId, PersonPaulaPartyId, 3),
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, OrganizationOrstaPartyId),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        PersonPaulaPartyId
    ]];

    /// <summary>
    /// a
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToOrganization() => [[
        PrincipalUtil.GetToken(PersonKasperUserId, PersonKasperPartyId, 3),
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, OrganizationKolbjornPartyId),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        OrganizationOrstaPartyId
    ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToPerson() => [[
        PrincipalUtil.GetToken(PersonKasperUserId, PersonKasperPartyId, 3),
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, PersonPaulaUserId),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        OrganizationOrstaPartyId
    ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToEnterpriseuser() => [[
        PrincipalUtil.GetToken(PersonKasperUserId, PersonKasperPartyId, 3),
        NewRevokeOfferedModel(
            WithRevokeOfferedTo(Urn.Altinn.EnterpriseUser.Username, EnterpriseUsername),
            WithRevokeOfferedAction("read"),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeOfferedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        OrganizationOrstaPartyId
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