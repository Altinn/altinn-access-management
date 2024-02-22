using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;

/// <summary>
/// testdata
/// </summary>
public static class TestDataRevokeReceivedDelegationExternal
{
    private static string PersonPaulaSSN => "02056260016";

    private static string PersonOrjanSSN => "27099450067";

    private static string ResourceOrg => "org1";

    private static string ResourceAppId => "app1";

    private static string OrganizationOrstadAccounting => "910459880";

    private static string OrganizationKolbjorn => "810419342";

    private static string EnterpriseUsername => "OrstaECUser";

    /// <summary>
    /// An input model that specifies revoking an existing delegation from Orjan to Paula.
    /// The delegation 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToPerson() => [[
            NewRevokeReceivedModel(
                WithRevokeReceivedFrom(Urn.Altinn.Person.IdentifierNo, PersonOrjanSSN),
                WithRevokeReceivedAction("read"),
                WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
                WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
            IdentifierUtil.PersonHeader,
            PersonPaulaSSN,
        ]];

    /// <summary>
    /// An input model that specifies an delegation from Paula
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromPersonToOrganization() => [[
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(Urn.Altinn.Person.IdentifierNo, PersonPaulaSSN),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        IdentifierUtil.OrganizationNumberHeader,
        OrganizationOrstadAccounting,
        ]];

    /// <summary>
    /// a
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToOrganization() => [[
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(Urn.Altinn.Organization.IdentifierNo, OrganizationOrstadAccounting),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        IdentifierUtil.OrganizationNumberHeader,
        OrganizationKolbjorn,
        ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToPerson() => [[
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(Urn.Altinn.Organization.IdentifierNo, OrganizationOrstadAccounting),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        IdentifierUtil.PersonHeader,
        PersonPaulaSSN,
    ]];

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FromOrganizationToEnterpriseuser() => [[
        NewRevokeReceivedModel(
            WithRevokeReceivedFrom(Urn.Altinn.Organization.IdentifierNo, OrganizationOrstadAccounting),
            WithRevokeReceivedAction("read"),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppOwner, ResourceOrg),
            WithRevokeReceivedResource(Urn.Altinn.Resource.AppId, ResourceAppId)),
        IdentifierUtil.PersonHeader,
        EnterpriseUsername,
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