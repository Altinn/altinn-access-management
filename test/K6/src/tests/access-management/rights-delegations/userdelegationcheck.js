/*
  Test data required: deployed app (reference app: ttd/apps-test)
  userid, partyid for two users that are DAGL for two orgs, and partyid and orgno for those orgs (user1 and user2)
  Org number for user2's org
docker-compose run k6 run /src/tests/access-management/rights-delegations.js -e env=*** -e tokengenuser=*** -e tokengenuserpwd=*** -e appsaccesskey=***


*/
import { check, sleep, fail } from 'k6';
import { addErrorCount, stopIterationOnFail } from '../../../errorcounter.js';
import { generateToken } from '../../../api/altinn-testtools/token-generator.js';
import { generateJUnitXML, reportPath } from '../../../report.js';
import * as userDelegationCheck from '../../../api/access-management/rights-delegations/userdelegationcheck.js';

const environment = __ENV.env.toLowerCase();
const tokenGeneratorUserName = __ENV.tokengenuser;
const tokenGeneratorUserPwd = __ENV.tokengenuserpwd;
let testdataFile = open(`../../../data/testdata/access-management/rights-delegations/${__ENV.env}testdata.json`);
var testdata = JSON.parse(testdataFile);
var org1;
var org2;

export const options = {
  thresholds: {
    errors: ['count<1'],
  },
  setupTimeout: '1m',
};

export function setup() {

  //generate personal token for user 1 (DAGL for org1)
  var tokenGenParams = {
    env: testdata.env,
    scopes: 'altinn:instances.read',
    pid: testdata.org1.dagl.pid,
    userid: testdata.org1.dagl.userid,
    partyid: testdata.org1.dagl.partyid,
    authLvl: 3,
  };

  testdata.org1.dagl.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: testdata.env,
    scopes: 'altinn:instances.read',
    pid: testdata.org2.dagl.pid,
    userid: testdata.org2.dagl.userid,
    partyid: testdata.org2.dagl.partyid,
    authLvl: 3,
  };
  testdata.org2.dagl.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: testdata.env,
    scopes: 'altinn:instances.read',
    pid: testdata.org1.hadm.pid,
    userid: testdata.org1.hadm.userid,
    partyid: testdata.org1.hadm.partyid,
    authLvl: 3,
  };
  testdata.org1.hadm.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: testdata.env,
    scopes: 'altinn:instances.read',
    pid: testdata.org1.apiadm.pid,
    userid: testdata.org1.apiadm.userid,
    partyid: testdata.org1.apiadm.partyid,
    authLvl: 3,
  };
  testdata.org1.apiadm.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  return testdata;
}

export default function (data) {
  if (!data) {
    return;
  }
  org1 = data.org1;
  org2 = data.org2;

  //tests
  daglForOrgHasScopeAccessForResource()
  daglStatusIsNotDelegableForPrivResource()
  hadmForOrgHasScopeAccess()
  privUserHasScopeAccessForResource()
  resourceIsNonDelegable()
  resourceDoesNotExist()
  daglForOrgHasScopeAccessForApp()
  daglStatusIsNotDelegableForPrivApp()
  privUserHasScopeAccessForApp()
  daglForOrgHasScopeAccessForAltinn2Service()
  privUserHasScopeAccessForAltinn2Service()
}

/** Checks that DAGl for an org has ScopeAccess for the resource k6-userdelegationcheck-apiadm */
export function daglForOrgHasScopeAccessForResource() {
    const offeredByToken = org1.dagl.token;
    const offeredByPartyId = org1.partyid;
    const appid = 'k6-userdelegationcheck-apiadm';
    var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);

      // Assert
  var success = check(res, {
    'daglForOrgHasScopeAccessForResource - status is 200': (r) => r.status === 200,
    'daglForOrgHasScopeAccessForResource - rightskey is ttd-am-k6:ScopeAccess': (r) => r.json('0.rightKey') === appid + ':ScopeAccess',
    'daglForOrgHasScopeAccessForResource - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
    'daglForOrgHasScopeAccessForResource - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
    'daglForOrgHasScopeAccessForResource - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'daglForOrgHasScopeAccessForResource - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
    'daglForOrgHasScopeAccessForResource - status is Delegable': (r) => r.json('0.status') == 'Delegable',
    'daglForOrgHasScopeAccessForResource - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
    'daglForOrgHasScopeAccessForResource - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
    'daglForOrgHasScopeAccessForResource - RoleRequirementsMatches value is APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'APIADM',
  });
  addErrorCount(success);
}

/** Checks that DAGl for an org can not delegate a resource that requires PRIV role */
export function daglStatusIsNotDelegableForPrivResource() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'k6-userdelegationcheck-priv';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);

    // Assert
var success = check(res, {
  'daglStatusIsNotDelegableForPrivResource - status is 200': (r) => r.status === 200,
  'daglStatusIsNotDelegableForPrivResource - rightskey is k6-userdelegationcheck-priv:ScopeAccess': (r) => r.json('0.rightKey') === 'k6-userdelegationcheck-priv:ScopeAccess',
  'daglStatusIsNotDelegableForPrivResource - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
  'daglStatusIsNotDelegableForPrivResource - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
  'daglStatusIsNotDelegableForPrivResource - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'daglStatusIsNotDelegableForPrivResource - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'daglStatusIsNotDelegableForPrivResource - status is NotDelegable': (r) => r.json('0.status') == 'NotDelegable',
  'daglStatusIsNotDelegableForPrivResource - code is MissingDelegationAccess': (r) => r.json('0.details.1.code') == 'MissingDelegationAccess',
  'daglStatusIsNotDelegableForPrivResource - code is MissingRoleAccess': (r) => r.json('0.details.0.code') == 'MissingRoleAccess',
  'daglStatusIsNotDelegableForPrivResource - RequiredRoles id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RequiredRoles.0.id') == 'urn:altinn:rolecode',
  'daglStatusIsNotDelegableForPrivResource - RequiredRoles value is PRIV': (r) => r.json('0.details.0.parameters.RequiredRoles.0.value') == 'PRIV',
});
addErrorCount(success);
}

/** Checks that HADM for an org has ScopeAccess for the resource k6-userdelegationcheck-apiadm */
export function hadmForOrgHasScopeAccess() {
  const offeredByToken = org1.hadm.token;
  const offeredByPartyId = org1.hadm.partyid;
  const appid = 'k6-userdelegationcheck-apiadm';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);

    // Assert
var success = check(res, {
  'hadmForOrgHasScopeAccess - status is 200': (r) => r.status === 200,
  'hadmForOrgHasScopeAccess - rightskey is ttd-am-k6:ScopeAccess': (r) => r.json('0.rightKey') === appid + ':ScopeAccess',
  'hadmForOrgHasScopeAccess - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
  'hadmForOrgHasScopeAccess - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
  'hadmForOrgHasScopeAccess - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'hadmForOrgHasScopeAccess - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'hadmForOrgHasScopeAccess - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'hadmForOrgHasScopeAccess - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'hadmForOrgHasScopeAccess - RoleRequirementsMatches is urn:altinn:rolecode:APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches') == 'urn:altinn:rolecode:APIADM',
});
addErrorCount(success);
}

/** Checks that a PRIV user has ScopeAccess for the a resource that requires the PRIV role */
export function privUserHasScopeAccessForResource() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.dagl.partyid;
  const appid = 'k6-userdelegationcheck-priv';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);

    // Assert
var success = check(res, {
  'privUserHasScopeAccessForResource - status is 200': (r) => r.status === 200,
  'privUserHasScopeAccessForResource - rightskey is k6-userdelegationcheck-priv:ScopeAccess': (r) => r.json('0.rightKey') === 'k6-userdelegationcheck-priv:ScopeAccess',
  'privUserHasScopeAccessForResource - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
  'privUserHasScopeAccessForResource - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
  'privUserHasScopeAccessForResource - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'privUserHasScopeAccessForResource - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'privUserHasScopeAccessForResource - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'privUserHasScopeAccessForResource - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'privUserHasScopeAccessForResource - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
  'privUserHasScopeAccessForResource - RoleRequirementsMatches value is PRIV': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'PRIV',
});
addErrorCount(success);
}

/** Checks that the response is 400 and with an error message when trying to delegate a non-delegable resource */
export function resourceIsNonDelegable() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'k6-userdelegationcheck-apiadm-non-delegable';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'ResourceIsNonDelegable - status is 400 ': (r) => r.status === 400,
  'ResourceIsNonDelegable - Error message is "The resource: Identifier: k6-userdelegationcheck-apiadm-non-delegable"': (r) => r.body.includes('The resource: Identifier: k6-userdelegationcheck-apiadm-non-delegable'),
});
addErrorCount(success);
}

/** Checks that the response is 400 and with an error message when trying to delegate a non-existing resource */
export function resourceDoesNotExist() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'jfkdsaljfdksjafkjdiejfoewjifeovneuwvbu4bvu4buvbvbdbvubduwsdhuh';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'ResourceIsNonDelegable - status is 400 ': (r) => r.status === 400,
  'ResourceIsNonDelegable - Error message is "The resource: Identifier: jfkdsaljfdksjafkjdiejfoewjifeovneuwvbu4bvu4buvbvbdbvubduwsdhuh"': (r) => r.body.includes('The resource: Identifier: jfkdsaljfdksjafkjdiejfoewjifeovneuwvbu4bvu4buvbvbdbvubduwsdhuh'),
});
addErrorCount(success);
}

/** Checks that DAGl for an org has ScopeAccess for the altinn3 app ttd/apps-test */
export function daglForOrgHasScopeAccessForApp() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const org = 'ttd';
  const app = 'apps-test';
  var res = userDelegationCheck.appUserDelegationCheck(offeredByToken, offeredByPartyId, org, app);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'daglForOrgHasScopeAccessForApp - status is 200': (r) => r.status === 200,
  'daglForOrgHasScopeAccessForApp - rightskey is apps-test,ttd:ScopeAccess': (r) => r.json('0.rightKey') === 'apps-test,ttd:ScopeAccess',
  'daglForOrgHasScopeAccessForApp - resource org id is urn:altinn:org': (r) => r.json('0.resource.0.id') === 'urn:altinn:org',
  'daglForOrgHasScopeAccessForApp - resource org value is ttd': (r) => r.json('0.resource.0.value') == org,
  'daglForOrgHasScopeAccessForApp - resource app id is urn:altinn:org': (r) => r.json('0.resource.1.id') === 'urn:altinn:app',
  'daglForOrgHasScopeAccessForApp - resource app value is apps-test-tba': (r) => r.json('0.resource.1.value') == app,
  'daglForOrgHasScopeAccessForApp - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'daglForOrgHasScopeAccessForApp - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'daglForOrgHasScopeAccessForApp - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'daglForOrgHasScopeAccessForApp - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches value is PRIV': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'APIADM',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches id is urn:altinn:org': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.1.id') == 'urn:altinn:org',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches value is ttd': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.1.value') == 'ttd',
});
addErrorCount(success);
}

/** Checks that a PRIV user has ScopeAccess for the altinn3 app ttd/apps-test-tba */
export function privUserHasScopeAccessForApp() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.dagl.partyid;
  const org = 'ttd';
  const app = 'apps-test-tba';
  var res = userDelegationCheck.appUserDelegationCheck(offeredByToken, offeredByPartyId, org, app);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'privUserHasScopeAccessForApp - status is 200': (r) => r.status === 200,
  'privUserHasScopeAccessForApp - rightskey is apps-test-tba,ttd:ScopeAccess': (r) => r.json('0.rightKey') === 'apps-test-tba,ttd:ScopeAccess',
  'privUserHasScopeAccessForApp - resource org id is urn:altinn:org': (r) => r.json('0.resource.0.id') === 'urn:altinn:org',
  'privUserHasScopeAccessForApp - resource org value is ttd': (r) => r.json('0.resource.0.value') == org,
  'privUserHasScopeAccessForApp - resource app id is urn:altinn:app': (r) => r.json('0.resource.1.id') === 'urn:altinn:app',
  'privUserHasScopeAccessForApp - resource app value is apps-test': (r) => r.json('0.resource.1.value') == app,
  'privUserHasScopeAccessForApp - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'privUserHasScopeAccessForApp - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'privUserHasScopeAccessForApp - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'privUserHasScopeAccessForApp - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches value is PRIV': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'PRIV',
});
addErrorCount(success);
}

/** Checks that DAGl for an org can not delegate an Altinn3 app that requires PRIV role */
export function daglStatusIsNotDelegableForPrivApp() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const org = 'ttd';
  const app = 'apps-test-tba';
  var res = userDelegationCheck.appUserDelegationCheck(offeredByToken, offeredByPartyId, org, app);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'daglStatusIsNotDelegableForPrivApp - status is 200': (r) => r.status === 200,
  'daglStatusIsNotDelegableForPrivApp - rightskey is apps-test-tba,ttd:ScopeAccess': (r) => r.json('0.rightKey') === 'apps-test-tba,ttd:ScopeAccess',
  'daglStatusIsNotDelegableForPrivApp - resource org id is urn:altinn:org': (r) => r.json('0.resource.0.id') === 'urn:altinn:org',
  'daglStatusIsNotDelegableForPrivApp - resource org value is ttd': (r) => r.json('0.resource.0.value') == org,
  'daglStatusIsNotDelegableForPrivApp - resource app id is urn:altinn:app': (r) => r.json('0.resource.1.id') === 'urn:altinn:app',
  'daglStatusIsNotDelegableForPrivApp - resource app value is apps-test': (r) => r.json('0.resource.1.value') == app,
  'daglStatusIsNotDelegableForPrivApp - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'daglStatusIsNotDelegableForPrivApp - action value is ScopeAccess': (r) => r.json('0.action.value') == 'ScopeAccess',
  'daglStatusIsNotDelegableForPrivApp - status is NotDelegable': (r) => r.json('0.status') == 'NotDelegable',
  'daglStatusIsNotDelegableForPrivApp - code is MissingRoleAccess': (r) => r.json('0.details.0.code') == 'MissingRoleAccess',
  'daglStatusIsNotDelegableForPrivApp - RequiredRoles id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RequiredRoles.0.id') == 'urn:altinn:rolecode',
  'daglStatusIsNotDelegableForPrivApp - RequiredRoles value is PRIV': (r) => r.json('0.details.0.parameters.RequiredRoles.0.value') == 'PRIV',
  'daglStatusIsNotDelegableForPrivApp - RequiredRoles id is urn:altinn:org': (r) => r.json('0.details.0.parameters.RequiredRoles.1.id') == 'urn:altinn:org',
  'daglStatusIsNotDelegableForPrivApp - RequiredRoles value is ttd': (r) => r.json('0.details.0.parameters.RequiredRoles.1.value') == 'ttd',
});
addErrorCount(success);
}

/** Checks that DAGl for an org has ScopeAccess for the altinn2 service ACC Security level 2 MAG */
export function daglForOrgHasScopeAccessForAltinn2Service() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.dagl.partyid;
  const serviceCode = '2802';
  const serviceEditionCode = '2203';
  var res = userDelegationCheck.altinn2ServiceUserDelegationCheck(offeredByToken, offeredByPartyId, serviceCode, serviceEditionCode);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'daglForOrgHasScopeAccessForAltinn2Service - status is 200': (r) => r.status === 200,
  'daglForOrgHasScopeAccessForAltinn2Service - rightskey is 2802:2203:read': (r) => r.json('0.rightKey') === '2802:2203:read',
  'daglForOrgHasScopeAccessForAltinn2Service - resource id is urn:altinn:servicecode': (r) => r.json('0.resource.0.id') === 'urn:altinn:servicecode',
  'daglForOrgHasScopeAccessForAltinn2Service - servicecode is 3225': (r) => r.json('0.resource.0.value') == serviceCode,
  'daglForOrgHasScopeAccessForAltinn2Service - resource id is urn:altinn:serviceeditioncode': (r) => r.json('0.resource.1.id') === 'urn:altinn:serviceeditioncode',
  'daglForOrgHasScopeAccessForAltinn2Service - serviceeditioncoee is 536': (r) => r.json('0.resource.1.value') == serviceEditionCode,
  'daglForOrgHasScopeAccessForAltinn2Service - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'daglForOrgHasScopeAccessForAltinn2Service - action value is read': (r) => r.json('0.action.value') == 'read',
  'daglForOrgHasScopeAccessForAltinn2Service - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'daglForOrgHasScopeAccessForAltinn2Service - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'daglForOrgHasScopeAccessForAltinn2Service - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
  'daglForOrgHasScopeAccessForAltinn2Service - RoleRequirementsMatches value is APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'UTINN',
});
addErrorCount(success);
}

/** Checks that a PRIV user has ScopeAccess for the altinn2 service ACC Security level 2 MAG */
export function privUserHasScopeAccessForAltinn2Service() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const serviceCode = '2802';
  const serviceEditionCode = '2203';
  var res = userDelegationCheck.altinn2ServiceUserDelegationCheck(offeredByToken, offeredByPartyId, serviceCode, serviceEditionCode);
  console.log(res.status);
  console.log(res.body);

    // Assert
var success = check(res, {
  'privUserHasScopeAccessForAltinn2Service - status is 200': (r) => r.status === 200,
  'privUserHasScopeAccessForAltinn2Service - rightskey is 2802:2203:read': (r) => r.json('0.rightKey') === '2802:2203:read',
  'privUserHasScopeAccessForAltinn2Service - resource id is urn:altinn:servicecode': (r) => r.json('0.resource.0.id') === 'urn:altinn:servicecode',
  'privUserHasScopeAccessForAltinn2Service - servicecode is 3225': (r) => r.json('0.resource.0.value') == serviceCode,
  'privUserHasScopeAccessForAltinn2Service - resource id is urn:altinn:serviceeditioncode': (r) => r.json('0.resource.1.id') === 'urn:altinn:serviceeditioncode',
  'privUserHasScopeAccessForAltinn2Service - serviceeditioncoee is 536': (r) => r.json('0.resource.1.value') == serviceEditionCode,
  'privUserHasScopeAccessForAltinn2Service - action id is urn:oasis:names:tc:xacml:1.0:action:action-id': (r) => r.json('0.action.id') == 'urn:oasis:names:tc:xacml:1.0:action:action-id',
  'privUserHasScopeAccessForAltinn2Service - action value is read': (r) => r.json('0.action.value') == 'read',
  'privUserHasScopeAccessForAltinn2Service - status is Delegable': (r) => r.json('0.status') == 'Delegable',
  'privUserHasScopeAccessForAltinn2Service - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
  'privUserHasScopeAccessForAltinn2Service - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
  'privUserHasScopeAccessForAltinn2Service - RoleRequirementsMatches value is APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'UTINN',
});
addErrorCount(success);
}

export function handleSummary(data) {
    let result = {};
    result[reportPath('userdelegationcheck.xml')] = generateJUnitXML(data, 'access-management-rights-delegations-userdelegationcheck');
    return result;
}