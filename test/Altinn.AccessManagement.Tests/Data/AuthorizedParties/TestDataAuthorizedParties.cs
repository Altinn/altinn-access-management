using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static System.Collections.Specialized.BitVector32;

namespace Altinn.AccessManagement.Tests.Data;

/// <summary>
/// Test data builder for testing AuthorizedParties
/// </summary>
public static class TestDataAuthorizedParties
{
#pragma warning disable SA1600 // Elements should be documented
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static string OnlyAltinn3 => "OnlyAltinn3";

    private static string BothAltinn3AndAltinn2 => "BothAltinn3AndAltinn2";

    private static string InclResourcesThroughRoles => "InclResourcesThroughRoles";

    public static int PersonToPerson_FromUserId => 20100001;

    public static int PersonToPerson_FromPartyId => 50100001;

    public static int PersonToPerson_ToUserId => 20100002;

    public static int PersonToPerson_ToPartyId => 50100002;

    public static int PersonToOrg_FromUserId => 20100003;

    public static int PersonToOrg_FromPartyId => 50100003;

    public static int PersonToOrg_ToOrgPartyId => 50100004;

    public static int PersonToOrg_ToOrgDaglUserId => 20100005;

    public static int PersonToOrg_ToOrgDaglPartyId => 50100005;

    public static int MainUnit_PartyId => 50100006;

    public static int SubUnit_PartyId => 50100007;

    public static int SubUnitTwo_PartyId => 50100008;

    public static int MainUnitAndSubUnitToPerson_ToUserId => 20100009;

    public static int MainUnitAndSubUnitToPerson_ToPartyId => 50100009;

    public static string MainUnitAndSubUnitToOrg_ToOrgOrganizationNumber => "901000010";

    public static string MainUnitAndSubUnitToOrg_ToOrgOrganizationUuid => "00000000-0000-0000-0001-000000000010";

    public static int MainUnitAndSubUnitToOrg_ToOrgPartyId => 50100010;

    public static string MainUnitAndSubUnitToOrg_ToOrgDaglPersonId => "01018170071";

    public static string MainUnitAndSubUnitToOrg_ToOrgDaglPersonUuid => "00000000-0000-0000-0001-000000000011";

    public static int MainUnitAndSubUnitToOrg_ToOrgDaglUserId => 20100011;

    public static int MainUnitAndSubUnitToOrg_ToOrgDaglPartyId => 50100011;

    public static string MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUsername => "MainUnit_And_SubUnit_ToOrg_EcKeyRole_User";

    public static string MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUserUuid => "00000000-0000-0000-0002-000000000010";

    public static int MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUserId => 20100010;

    public static int SubUnitToPerson_ToUserId => 20100012;

    public static int SubUnitToPerson_ToPartyId => 50100012;

#pragma warning restore SA1600 // Elements should be documented

    /// <summary>
    /// Sets up a request without a valid token
    /// </summary>
    public static TheoryData<string> UnauthenticatedNoValidToken() => new()
    {
        {
            string.Empty
        }
    };

    /// <summary>
    /// Sets up a request with a valid token but missing a valid urn:altinn:userid claim
    /// </summary>
    public static TheoryData<string> UnauthenticatedValidTokenMissingUserContext() => new()
    {
        {
            PrincipalUtil.GetToken(0, 0, 0)
        }
    };

    /// <summary>
    /// Sets up a request with a valid token but missing a valid authorization scope for authorized party list API
    /// </summary>
    public static TheoryData<string> ValidResourceOwnerTokenMissingScope() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:serviceowner")
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToPerson() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToPerson", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToPersonInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToPerson", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToOrg() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToOrg", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToOrgInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToOrg", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-person
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToPerson() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToPerson", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-person
    ///         the role: REGNA
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    ///         the role: SISKD
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToPersonInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToPerson", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-org
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-org
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToOrg() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToOrg", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-org
    ///         the role: UTINN
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-org
    ///         the role: APIADM
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToOrgInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToOrg", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    ///         the resource: devtest_gar_authparties-sub-to-person
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> SubUnitToPerson() => new()
    {
        {
            PrincipalUtil.GetToken(SubUnitToPerson_ToUserId, SubUnitToPerson_ToPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("SubUnitToPerson", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting it's own authorized party
    /// without including Altinn 2 authorized parties
    /// 
    /// Expected result: empty response since Altinn 2 is required in order to get self through PRIV role
    /// </summary>
    public static TheoryData<string, int, bool, ValidationProblemDetails> PersonGettingSelf_BadRequest() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            false,
            GetExpectedResponse<ValidationProblemDetails>("PersonGettingSelf", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting it's own authorized party
    /// including Altinn 2 authorized parties
    /// 
    /// Expected result: success, own authorized party is found including the PRIV authorized role
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingSelfInclA2_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET AuthorizedParty/{A3DelegatorPartyId} for authenticated user, without including Altinn 2 authorized parties
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party model of the authenticated user
    /// Reason:     Since Altinn 3 delegation exists A2 authorized parties is not needed to find Authorized Party with an authorized resource connection
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingA3Delegator_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET AuthorizedParty/{A3DelegatorPartyId} for authenticated user, including Altinn 2 authorized parties
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party model of the authenticated user
    /// Reason:     Since Altinn 3 delegation exists Altinn 2 authorized parties is not needed to find Authorized Party with an authorized resource connection,
    ///             but will enrich the Authorized Party with any authorized roles from Altinn 2
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingA3DelegatorInclA2_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user and getting the authorized party list for it's own {party}
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the authenticated user
    /// Reason:     Authenticated users are authorized for getting own authorized party list
    /// </summary>
    public static TheoryData<string, int, bool, List<AuthorizedPartyExternal>> PersonGettingOwnList_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonGettingOwnList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user being an authorized Access Manager for the {party} organization
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the organization
    /// Reason:     Authenticated users who are authorized as Access Manager for organizations should be allowed to get the organizations authorized party list
    /// </summary>
    public static TheoryData<string, int, bool, List<AuthorizedPartyExternal>> AccessManagerGettingOrgList_Success() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            MainUnitAndSubUnitToOrg_ToOrgPartyId,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("AccessManagerGettingOrgList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user being an authorized Access Manager for the {party} person
    /// Expected:   - Should return 403 Forbidden
    ///             - Should include expected authorized party list of the organization
    /// Reason:     Authenticated users who are authorized as Access Manager for organizations should be allowed to get the organizations authorized party list
    /// </summary>
    public static TheoryData<string, int, bool> AccessManagerGettingPersonList_Forbidden() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_FromPartyId,
            true
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for a person identified by urn:altinn:person:identifier-no
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetPersonList_ByPersonId() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId, Value = MainUnitAndSubUnitToOrg_ToOrgDaglPersonId },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetPersonList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  <![CDATA[POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}&includeAuthorizedResourcesThroughRoles={inclRoleResources}]]>
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for a person identified by urn:altinn:person:identifier-no
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    ///             - Should include authorized resources the user has access to through roles for each authorized party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetPersonList_ByPersonId_InclResourcesThroughRoles() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId, Value = MainUnitAndSubUnitToOrg_ToOrgDaglPersonId },
            false,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetPersonList", InclResourcesThroughRoles)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for a person identified by urn:altinn:person:uuid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetPersonList_ByPersonUuid() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid, Value = MainUnitAndSubUnitToOrg_ToOrgDaglPersonUuid },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetPersonList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for a person identified by urn:altinn:partyid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetPersonList_ByPartyId() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = MainUnitAndSubUnitToOrg_ToOrgDaglPartyId.ToString() },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetPersonList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for a person identified by urn:altinn:userid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetPersonList_ByUserId() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = MainUnitAndSubUnitToOrg_ToOrgDaglUserId.ToString() },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetPersonList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:organization:identifier-no
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetOrgList_ByOrganizationNumber() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationId, Value = MainUnitAndSubUnitToOrg_ToOrgOrganizationNumber },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetOrgList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  <![CDATA[POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}&includeAuthorizedResourcesThroughRoles={inclRoleResources}]]>
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:organization:identifier-no
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    ///             - Should include authorized resources the user has access to through roles for each authorized party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetOrgList_ByOrganizationNumber_InclResourcesThroughRoles() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationId, Value = MainUnitAndSubUnitToOrg_ToOrgOrganizationNumber },
            false,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetOrgList", InclResourcesThroughRoles)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:organization:uuid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetOrgList_ByOrganizationUuid() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid, Value = MainUnitAndSubUnitToOrg_ToOrgOrganizationUuid },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetOrgList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:partyid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetOrgList_ByPartyId() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = MainUnitAndSubUnitToOrg_ToOrgPartyId.ToString() },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetOrgList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:enterpriseuser:username
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUsername() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName, Value = MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUsername },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetEnterpriseUserList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  <![CDATA[POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}&includeAuthorizedResourcesThroughRoles={inclRoleResources}]]>
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:enterpriseuser:username
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    ///             - Should include authorized resources the user has access to through roles for each authorized party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUsername_InclResourcesThroughRoles() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName, Value = MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUsername },
            false,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetEnterpriseUserList", InclResourcesThroughRoles)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:enterpriseuser:uuid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUuid() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid, Value = MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUserUuid },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetEnterpriseUserList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:userid
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, BaseAttributeExternal, bool, bool, List<AuthorizedPartyExternal>> ResourceOwner_GetEnterpriseUserList_ByUserId() => new()
    {
        {
            PrincipalUtil.GetOrgToken("digdir", "991825827", "altinn:accessmanagement/authorizedparties.resourceowner"),
            new BaseAttributeExternal { Type = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = MainUnitAndSubUnitToOrg_ToOrgEcKeyRoleUserId.ToString() },
            true,
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("ResourceOwner_GetEnterpriseUserList", BothAltinn3AndAltinn2)
        }
    };

    private static T GetExpectedResponse<T>(string delegationType, string retrievalType)
    {
        string content = File.ReadAllText($"Data/Json/AuthorizedParties/{delegationType}/{retrievalType}.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }

    /// <summary>
    /// Assert that two <see cref="AuthorizedParty"/> have the same property in the same positions.
    /// </summary>
    /// <param name="expected">An instance with the expected values.</param>
    /// <param name="actual">The instance to verify.</param>
    public static void AssertAuthorizedPartyExternalEqual(AuthorizedPartyExternal expected, AuthorizedPartyExternal actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.Equal(expected.PartyId, actual.PartyId);
        Assert.Equal(expected.PartyUuid, actual.PartyUuid);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.OrganizationNumber, actual.OrganizationNumber);
        Assert.Equal(expected.PersonId, actual.PersonId);
        Assert.Equal(expected.UnitType, actual.UnitType);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.IsDeleted, actual.IsDeleted);
        Assert.Equal(expected.OnlyHierarchyElementWithNoAccess, actual.OnlyHierarchyElementWithNoAccess);
        Assert.Equal(expected.AuthorizedResources, actual.AuthorizedResources);
        Assert.Equal(expected.AuthorizedRoles, actual.AuthorizedRoles);
        AssertionUtil.AssertCollections(expected.Subunits, actual.Subunits, AssertAuthorizedPartyExternalEqual);
    }
}