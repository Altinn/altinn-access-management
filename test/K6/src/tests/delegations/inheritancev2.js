/*
  Test data required: deployed app (reference app: ttd/apps-test)
  Username and password for a user with the DAGL role for an organization (user1 and user2)
  Username and password for a user with the DAGL role for an organization with subunits (user3)
  Username, password, and org number for an enterprise user (ecusername, ecuserpwd, ecuserorgno)
  Org number for user2's org (same as org number for the enterprise user)
  Command: docker-compose run k6 run /src/tests/platform/authorization/delegations/inheritancev2.js
  -e env=*** -e tokengenuser=*** -e tokengenuserpwd=*** -e appsaccesskey=***
*/
import { check, sleep, fail } from 'k6';
import { addErrorCount, stopIterationOnFail } from '../../errorcounter.js';
import * as delegation from '../../api/platform/authorization/delegations.js';
import { generateToken } from '../../api/altinn-testtools/token-generator.js';
import { generateJUnitXML, reportPath } from '../../report.js';
import * as helper from '../../Helpers/TestdataHelper.js';

const environment = __ENV.env.toLowerCase();
const tokenGeneratorUserName = __ENV.tokengenuser;
const tokenGeneratorUserPwd = __ENV.tokengenuserpwd;

let testDataFile = open(`../../data/testdata/delegations/${environment}testdata.json`);
var testdata = JSON.parse(testDataFile);
var org1;
var org2;
var org3;
var token;
var org;
var app;
var showResults;

export const options = {
  thresholds: {
    errors: ['count<1'],
  },
  setupTimeout: '1m',
};

export function setup() {
  var tokenGenParams = {
    env: environment,
    app: 'sbl.authorization',
  };

  testdata.token = generateToken('platform', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);
  return testdata;

}

//Tests for platform Authorization:Delegations:Inheritance
export default function (data) {
  org1 = data.org1;
  org2 = data.org2;
  org3 = data.org3;
  token = data.token;
  org = data.org;
  app = data.app;
  showResults = 0;

  CleanupBeforeTests();

  //tests
  directDelegationFromOrgToUser();
  directDelegationFromOrgToOrg();
  directDelegationFromMainUnitToUser();
  directDelegationFromMainUnitToOrg();
  directDelegationFromMainUnitToOrgInheritedByDAGLViaKeyRole();
  delegationToOrgIsInheritedByECUserViaKeyrole();
}

export function CleanupBeforeTests() {
  helper.deleteAllRules(token, org1.dagl.userid, org1.partyid, org2.dagl.userid, 'userid', org, app);
  helper.deleteAllRules(token, org1.dagl.userid, org1.partyid, org2.partyid, 'partyid', org, app);
  helper.deleteAllRules(token, org3.dagl.userid, org3.partyid, org2.dagl.userid, 'userid', org, app);
  helper.deleteAllRules(token, org3.dagl.userid, org3.partyid, org2.partyid, 'partyid', org, app);
}

/**
 * Tests that an organization (org1) can successfully delegate directly to a user (user2)
 */
export function directDelegationFromOrgToUser() {
  // Arrange
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);

  // Assert
  var success = check(res, {
    'Direct delegation from org to user - status is 200': (r) => r.status === 200,
    'Direct delegation from org to user - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Direct delegation from org to user - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Direct delegation from org to user - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'Direct delegation from org to user - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:userid',
    'Direct delegation from org to user - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByUserId.toString(),
    'Direct delegation from org to user - type is 1': (r) => r.json('0.type') === 1,
  });
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByUserId, 'userid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  success = check(res, {
    'Direct delegation from org to user - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from org to user - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('directDelegationFromOrgToUser:' + success)}

  sleep(3);
}

/**
 * Tests that an organization (org1) can successfully delegate directly to another organization (org2)
 */
export function directDelegationFromOrgToOrg() {
  // Arrange
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByPartyId = org2.partyid;
  const DAGLUserIdForCoveredBy= org2.dagl.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByPartyId, coveredByPartyId, 'partyid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:partyid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByPartyId, resources, null, null);


  // Assert
  var success = check(res, {
    'Direct delegation from org to org - status is 200': (r) => r.status === 200,
    'Direct delegation from org to org - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Direct delegation from org to org - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Direct delegation from org to org - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'Direct delegation from org to org - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:partyid',
    'Direct delegation from org to org - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByPartyId.toString(),
    'Direct delegation from org to org - type is 1': (r) => r.json('0.type') === 1,
  });
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByPartyId, 'partyid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByPartyId, resources, null, null);
  success = check(res, {
    'Direct delegation from org to org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from org to org - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('directDelegationFromOrgToOrg:' + success)}
  sleep(3);
}

/**
 * Tests that when an organization (org3) delegates to a user (user2), that user also has access to the organization's subunit (org4)
 */
export function directDelegationFromMainUnitToUser() {
  // Arrange
  const performedByUserId = org3.dagl.userid;
  const offeredByParentPartyId = org3.partyid;
  const subUnitPartyId = org3.subunit.orgno;
  const coveredByUserId = org2.dagl.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByParentPartyId, coveredByUserId, 'userid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, coveredByUserId, resources, offeredByParentPartyId, null);

  // Assert
  var success = check(res, {
    'Direct delegation from mainunit to user - status is 200': (r) => r.status === 200,
    'Direct delegation from mainunit to user - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Direct delegation from mainunit to user - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Direct delegation from mainunit to user - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByParentPartyId,
    'Direct delegation from mainunit to user - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:userid',
    'Direct delegation from mainunit to user - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByUserId.toString(),
    'Direct delegation from mainunit to user - type is 3': (r) => r.json('0.type') === 3,
  });
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByParentPartyId, coveredByUserId, 'userid', org, app);
  res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, coveredByUserId, resources, offeredByParentPartyId, null);
  success = check(res, {
    'Direct delegation from mainunit to user - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from mainunit to user - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('directDelegationFromMainUnitToUser:' + success)}
  sleep(3);
}

/**
 * Tests that when an organization (org3) delegates to another org (org2), that the DAGL of that org also has access to the subunit (org4)
 */
export function directDelegationFromMainUnitToOrg() {
  // Arrange
  const performedByUserId = org3.dagl.userid;
  const offeredByParentPartyId = org3.partyid;
  const subUnitPartyId = org3.subunit.orgno;
  const coveredByPartyId = org2.partyid;
  const DAGLUserIdForCoveredBy= org2.dagl.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByParentPartyId, coveredByPartyId, 'partyid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:partyid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, coveredByPartyId, resources, offeredByParentPartyId, null);

  // Assert
  var success = check(res, {
    'Direct delegation from mainunit to org - status is 200': (r) => r.status === 200,
    'Direct delegation from mainunit to org - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Direct delegation from mainunit to org - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Direct delegation from mainunit to org - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByParentPartyId,
    'Direct delegation from mainunit to org - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:partyid',
    'Direct delegation from mainunit to org - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByPartyId.toString(),
    'Direct delegation from mainunit to org - type is 3': (r) => r.json('0.type') === 3,
  });
  addErrorCount(success);


  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByParentPartyId, coveredByPartyId, 'partyid', org, app);
  res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, coveredByPartyId, resources, offeredByParentPartyId, null);
  success = check(res, {
    'Direct delegation from mainunit to org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from mainunit to org - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('directDelegationFromMainUnitToOrg:' + success)}
  sleep(3);
}

/**
 * Tests that when an organization (org3) delegates to another org (org2), that the DAGL of that org also has access to the subunit (org4)
 */
export function directDelegationFromMainUnitToOrgInheritedByDAGLViaKeyRole() {
  // Arrange
  const performedByUserId = org3.dagl.userid;
  const offeredByParentPartyId = org3.partyid;
  const subUnitPartyId = org3.subunit.orgno;
  const coveredByPartyId = org2.partyid;
  const DAGLUserIdForCoveredBy= org2.dagl.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByParentPartyId, coveredByPartyId, 'partyid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, DAGLUserIdForCoveredBy, resources, offeredByParentPartyId, [coveredByPartyId]);
  // Assert
  var success = check(res, {
    'mainunit to org inherited by DAGL via keyrole - status is 200': (r) => r.status === 200,
    'mainunit to org inherited by DAGL via keyrole - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'mainunit to org inherited by DAGL via keyrole - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'mainunit to org inherited by DAGL via keyrole - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByParentPartyId,
    'mainunit to org inherited by DAGL via keyrole - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:partyid',
    'mainunit to org inherited by DAGL via keyrole - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByPartyId.toString(),
    'mainunit to org inherited by DAGL via keyrole - type is 4': (r) => r.json('0.type') === 4,
  });
  addErrorCount(success);


  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByParentPartyId, coveredByPartyId, 'partyid', org, app);
  res = delegation.getRules(token, policyMatchKeys, subUnitPartyId, DAGLUserIdForCoveredBy, resources, offeredByParentPartyId, [coveredByPartyId]);
  success = check(res, {
    'mainunit to org inherited by DAGL via keyrole - rules successfully deleted, status is 200': (r) => r.status == 200,
    'mainunit to org inherited by DAGL via keyrole - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('directDelegationFromMainUnitToOrgInheritedByDAGLViaKeyRole:' + success)}
  sleep(3);
}

/**
 * Verifies that when a delegation is made from one org (org1) to another (org2), the Enterprise Certificate user (ECUser) for that organization is also given access
 */
export function delegationToOrgIsInheritedByECUserViaKeyrole() {
  // Arrange
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByPartyId = org2.partyid;
  const ecUserIdForCoveredBy= org2.ecuser.userid;
  var resources = [{ appOwner: org, appName: app }];
  var ruleId = helper.addRulesForTest(token, performedByUserId, offeredByPartyId, coveredByPartyId, 'partyid', 'Task_1', 'read', org, app);

  // Act
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, ecUserIdForCoveredBy, resources, null, [coveredByPartyId]);

  // Assert
  var success = check(res, {
    'Delegation to Org is inherited by ECUser via keyrole - status is 200': (r) => r.status === 200,
    'Delegation to Org is inherited by ECUser via keyrole - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Delegation to Org is inherited by ECUser via keyrole - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Delegation to Org is inherited by ECUser via keyrole - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'Delegation to Org is inherited by ECUser via keyrole - coveredBy is userid': (r) => r.json('0.coveredBy.0.id') === 'urn:altinn:partyid',
    'Delegation to Org is inherited by ECUser via keyrole - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByPartyId.toString(),
    'Delegation to Org is inherited by ECUser via keyrole - type is 2': (r) => r.json('0.type') === 2,
  });
  addErrorCount(success);

  // Cleanup
  policyMatchKeys.coveredBy = 'urn:altinn:partyid';
  helper.deleteAllRules(token, org1.dagl.userid, org1.partyid, org2.partyid, 'partyid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, ecUserIdForCoveredBy, resources, null, [coveredByPartyId]);
  success = check(res, {
    'Delegation to Org is inherited by ECUser via keyrole - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Delegation to Org is inherited by ECUser via keyrole - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('delegationToOrgIsInheritedByECUserViaKeyrole:' + success)}
}

export function handleSummary(data) {
  let result = {};
  result[reportPath('authzDelegationInheritancev2.xml')] = generateJUnitXML(data, 'platform-authorization-delegation-inheritance-v2');
  return result;
}

export function showTestData() {
  console.log('environment: ' + environment);
  // console.log('altinnBuildVersion: ' + altinnBuildVersion);
  console.log('org1.orgno ' + org1.orgno);
  console.log('org1.partyid ' + org1.partyid);
  console.log('org2.orgno ' + org2.orgno);
  console.log('org2.partyid ' + org2.partyid);
  console.log('org3.orgno ' + org3.orgno);
  console.log('org3.partyid ' + org3.partyid);
  console.log('org3.subunit.orgno ' + org3.subunit.orgno);
  console.log('org3.subunit.orgno ' + org3.subunit.orgno);
  console.log('org1.dagl.userid ' + org1.dagl.userid);
  console.log('org1.dagl.partyid ' + org1.dagl.partyid);
  console.log('org2.dagl.userid ' + org2.dagl.userid);
  console.log('org2.dagl.partyid ' + org2.dagl.partyid);
  console.log('org3.dagl.userid ' + org3.dagl.userid);
  console.log('org3.dagl.partyid ' + org3.dagl.partyid);
  console.log('org2.ecuser.userid ' + org2.ecuser.userid);
  console.log('org2.ecuser.partyid ' + org2.ecuser.partyid);
  console.log('token: ' + token);
}
