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
import * as userDelegationCheck from '../../../api/platform/access-management/rights-delegations/userdelegationcheck.js';

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
  hadmForOrgHasScopeAccess()
  privUserHasScopeAccessForResource()
  resourceIsNonDelegable()
  daglForOrgHasScopeAccessForApp()
  privUserHasScopeAccessForApp()
}

// testing if testdata is correct
export function daglForOrgHasScopeAccessForResource() {
    const offeredByToken = org1.dagl.token;
    const offeredByPartyId = org1.partyid;
    const appid = 'k6-userdelegationcheck-apiadm';
    var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);
    console.log(res.status);
    console.log(res.body);

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
    'daglForOrgHasScopeAccessForResource - RoleRequirementsMatches is urn:altinn:rolecode:APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches') == 'urn:altinn:rolecode:APIADM',
  });
  addErrorCount(success);
}

// testing if testdata is correct
export function hadmForOrgHasScopeAccess() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.hadm.partyid;
  const appid = 'k6-userdelegationcheck-apiadm';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);
  console.log(res.status);
  console.log(res.body);

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

export function privUserHasScopeAccessForResource() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.dagl.partyid;
  const appid = 'k6-userdelegationcheck-priv';
  var res = userDelegationCheck.resourceUserDelegationCheck(offeredByToken, offeredByPartyId, appid);
  console.log(res.status);
  console.log(res.body);

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
  'privUserHasScopeAccessForResource - RoleRequirementsMatches is urn:altinn:rolecode:APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches') == 'urn:altinn:rolecode:PRIV',
});
addErrorCount(success);
}

// testing if testdata is correct
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

// testing if testdata is correct
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
  'daglForOrgHasScopeAccessForApp - RoleRequirementsMatches is urn:altinn:rolecode:APIADM, urn:altinn:org:ttd': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches') == 'urn:altinn:rolecode:APIADM, urn:altinn:org:ttd',
});
addErrorCount(success);
}

// testing if testdata is correct
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
  'privUserHasScopeAccessForApp - RoleRequirementsMatches is urn:altinn:rolecode:PRIV, urn:altinn:org:ttd': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches') == 'urn:altinn:rolecode:PRIV, urn:altinn:org:ttd',
});
addErrorCount(success);
}

export function handleSummary(data) {
    let result = {};
    result[reportPath('userdelegationcheck.xml')] = generateJUnitXML(data, 'access-management-rights-delegations-userdelegationcheck');
    return result;
}