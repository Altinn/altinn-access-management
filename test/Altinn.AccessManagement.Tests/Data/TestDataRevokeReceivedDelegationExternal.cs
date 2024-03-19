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
public static class TestDataRevokeReceivedDelegationExternal
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
    /// An input model that specifies revoking an existing delegation from Orjan to Paula.
    /// The delegation 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToPerson() => [[
        PrincipalUtil.GetToken(PersonOrjanUserId, PersonOrjanPartyId, 3),
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, PersonPaulaUserId),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        PersonOrjanPartyId
    ]];

    /// <summary>
    /// An input model that specifies an delegation from Paula
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToOrganization() => [[
        PrincipalUtil.GetToken(PersonOrjanUserId, PersonOrjanPartyId, 3),
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, PersonPaulaUserId),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        OrganizationOrstaPartyId
    ]];

    /// <summary>
    /// a
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToOrganization() => [[
        PrincipalUtil.GetToken(PersonOrjanUserId, PersonOrjanPartyId, 3),
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, OrganizationOrstaPartyId),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        OrganizationKolbjornPartyId
    ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToPerson() => [[
        PrincipalUtil.GetToken(PersonPaulaUserId, PersonPaulaPartyId, 3),
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, OrganizationOrstaPartyId),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        PersonPaulaPartyId
    ]];

    private static RevokeReceivedDelegationExternal NewRevokeReceivedModel(params Action<RevokeReceivedDelegationExternal>[] actions)
    {
        var model = new RevokeReceivedDelegationExternal();
        foreach (var action in actions)
        {
            action(model);
        }

        return model;
    }

    private static Action<RevokeReceivedDelegationExternal> WithRevokeReceivedAction(string action) => model =>
    {
        model.Rights ??= [new() { Action = action }];
        model.Rights.First().Action = action;
    };

    private static Action<RevokeReceivedDelegationExternal> WithRevokeReceivedResource(string id, object value) => model =>
    {
        model.Rights ??= [new() { Resource = [] }];
        model.Rights.First().Resource ??= [];
        model.Rights.First().Resource.Add(new()
        {
            Id = id,
            Value = value.ToString(),
        });
    };

    private static Action<RevokeReceivedDelegationExternal> WithRevokeReceivedFrom(string id, object value) => model =>
    {
        model.From ??= [];
        model.From.Add(new()
        {
            Id = id,
            Value = value.ToString(),
        });
    };
}