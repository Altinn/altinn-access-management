/*
  Test data required: deployed app (reference app: ttd/apps-test)
  Username and password for a user with the DAGL role for an organization (user1 and user2)
  Org number for user2's org
  Command: docker-compose run k6 run /src/tests/platform/authorization/delegations/delegationsv2.js 
  -e env=*** -e tokengenuser=*** -e tokengenuserpwd=*** -e appsaccesskey=*** 
  -e showresults=***

*/
import { check, sleep, fail } from 'k6';
import { addErrorCount, stopIterationOnFail } from '../../errorcounter.js';
import { generateToken } from '../../api/altinn-testtools/token-generator.js';
import { generateJUnitXML, reportPath } from '../../report.js';
import * as delegation from '../../api/platform/authorization/delegations.js';
import * as helper from '../../Helpers/TestdataHelper.js';

let pdpInputJson = open('../../data/pdpinput.json');

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
  getPolicyOfAnApp();
  addReadAccessToUserThenDeleteIt();
  addGetDeleteRuleAndCheckDecisions();
  delegateTwoRulesInOneRequest();
  delegateTwoRulesPartialSuccess();
  delegateRuleToAUserAndOrg();
}

export function CleanupBeforeTests() {
  helper.deleteAllRules(token, org1.dagl.userid, org1.partyid, org2.dagl.userid, 'userid', org, app);
  helper.deleteAllRules(token, org1.dagl.userid, org1.partyid, org2.partyid, 'partyid', org, app);
}

/** Retrieve policy of an app */
export function getPolicyOfAnApp() { 
  var resources = [{ appOwner: org, appName: app }];
  var res = delegation.getPolicies(resources);
  var success = check(res, {
    'GET app policy - status is 200': (r) => r.status === 200,
  });
  addErrorCount(success);
  if(showResults == 1) { console.log('getPolicyOfAnApp: ' + success) }
}

/** Add read access to a user for app in a particular task */
export function addReadAccessToUserThenDeleteIt() {
  // Arrange
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;

  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org', 'urn:altinn:task'],
  };

  // Act
  var res = delegation.addRules(token, policyMatchKeys, performedByUserId, offeredByPartyId, coveredByUserId, org, app, 'Task_1', 'read');
  var success = check(res, {
    'Add delegation rule - status is 201': (r) => r.status === 201,
    'Add delegation rule - rule id is not empty': (r) => r.json('0.ruleId') != null,
    'Add delegation rule - createdSuccessfully is true': (r) => r.json('0.createdSuccessfully') === true,
    'Add delegation rule - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'Add delegation rule - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByUserId.toString(),
  });

  // Assert
  addErrorCount(success);
  stopIterationOnFail('Add delegation rule Failed', success, res);
  var ruleId = res.json('0.ruleId');
  sleep(3);

  // Act (deletion)
  res = delegation.deleteRules(token, policyMatchKeys, [ruleId], performedByUserId, offeredByPartyId, coveredByUserId, org, app, 'Task_1', 'read');
      
  // Assert (deletion)
  success = check(res, {
    'Delete delegated rule - status is 200': (r) => r.status === 200,
  });
  addErrorCount(success);
  if(showResults == 1) { console.log('addReadAccessToUserThenDeleteIt: ' + success) }
}

/** Deleting a non existing rules fails */
export function deletingNonExistingRuleFails() {
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org', 'urn:altinn:task'],
  };

  var res = delegation.deleteRules(token, policyMatchKeys, ['12345678-a1b2-1234-1a23-1234a56b78c9'], org1.dagl.userid, org1.partyid, org2.dagl.userid, org, app, 'Task_1', 'read');
  var success = check(res, {
    'Delete a not existing rule - status is 400': (r) => r.status === 400,
  });
  addErrorCount(success);
  if(showResults == 1) { console.log('deletingNonExistingRuleFails: ' + success) }
}

/** Rules cannot be delegated with invalid app details */
export function addingRuleWithInvalidValuesFails() {
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org', 'urn:altinn:task'],
  };
  var res = delegation.addRules(token, policyMatchKeys, org1.dagl.userid, org1.partyid, org2.dagl.userid, org, app, 'test', 'Task_1', 'read');
  var success = check(res, {
    'Add delegation rule for an invalid app - status is 400': (r) => r.status === 400,
    'Add delegation rule for an invalid app - failed': (r) => r.body == 'Delegation could not be completed',
  });
  addErrorCount(success);
  if(showResults == 1) { console.log('addRuleWithInvalidValuesFails: ' + success) }
}

export function addGetDeleteRuleAndCheckDecisions() {
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;

  var resources = [{ appOwner: org, appName: app }];
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org', 'urn:altinn:task'],
  };
    //add a rule to give write access
  var res = delegation.addRules(token, policyMatchKeys, performedByUserId, offeredByPartyId, coveredByUserId, org, app, 'Task_1', 'write');
  var ruleId = res.json('0.ruleId');
  sleep(3);
  
  //Retrieve all the rules that are delegated to an user from a party
  policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  var success = check(res, {
    'Get delegated rule - status is 200': (r) => r.status === 200,
    'Get delegated rule - rule id matches': (r) => r.json('0.ruleId') === ruleId,
    'Get delegated rule - createdSuccessfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Get delegated rule - offeredByPartyId matches': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'Get delegated rule - coveredBy matches': (r) => r.json('0.coveredBy.0.value') === coveredByUserId.toString(),
    'Get delegated rule - type is 1': (r) => r.json('0.type') === 1,
  });
  addErrorCount(success);
  
  //Delete all the delegated rules from an user by a party
  res = delegation.deletePolicy(token, policyMatchKeys, performedByUserId, offeredByPartyId, coveredByUserId, org, app, null);
  success = check(res, {
    'Delete delegated policy with all rules - status is 200': (r) => r.status === 200,
  });
  addErrorCount(success);
  sleep(3);
  
  //Get rules that are deleted where response should be an empty array
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  success = check(res, {
    'Get deleted rules - status is 200': (r) => r.status === 200,
    'Get deleted rules - response is empty': (r) => r.json().length === 0,
  });
  addErrorCount(success);
}

export function delegateTwoRulesInOneRequest() {
  // Arrange
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var resources = [{ appOwner: org, appName: app }];
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;
  var rulesList = [];
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'read', org, app));
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'write', org, app));
  
  // Act
  var res = delegation.addMultipleRules(token, rulesList);
  
  // Assert
  var success = check(res, {
    'Add multiple rules success - status is 201': (r) => r.status === 201,
    'Add multiple rules success - rule 1 created successfully is true': (r) => r.json('0.createdSuccessfully') === true,
    'Add multiple rules success - rule 2 created successfully is true': (r) => r.json('1.createdSuccessfully') === true
  });
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByUserId, 'userid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  success = check(res, {
    'Direct delegation from org to org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from org to org - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('delegateTwoRulesInOneRequest:' + success)}
  sleep(3);

}

export function delegateTwoRulesPartialSuccess() {

  // Arrange
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var resources = [{ appOwner: org, appName: app }];
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;
  var rulesList = [];
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'read','ttd','nonExistentApp', org, app));
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'write', org, app));
  
  // Act
  var res = delegation.addMultipleRules(token, rulesList);
  
  // Assert
  var success = check(res, {
    'Add multiple rules partial - status is 206': (r) => r.status === 206,
    'Add multiple rules partial - rule 1 created successfully is false': (r) => r.json('0.createdSuccessfully') === false,
    'Add multiple rules partial - rule 2 created successfully is true': (r) => r.json('1.createdSuccessfully') === true
  });
  
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByUserId, 'userid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  success = check(res, {
    'Direct delegation from org to org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Direct delegation from org to org - rules successfully deleted, body is empty': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('delegateTwoRulesPartialSuccess:' + success)}
  sleep(3);

}
  
export function delegateRuleToAUserAndOrg() {

  // Arrange
  var policyMatchKeys = {
    coveredBy: 'urn:altinn:userid',
    resource: ['urn:altinn:app', 'urn:altinn:org'],
  };
  var resources = [{ appOwner: org, appName: app }];
  const performedByUserId = org1.dagl.userid;
  const offeredByPartyId = org1.partyid;
  const coveredByUserId = org2.dagl.userid;
  const coveredByPartyId =org2.partyid;
  var rulesList = [];
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByUserId, 'userid', 'Task_1', 'read', org, app));
  rulesList.push(helper.generateDataForAddMultipleRules(performedByUserId, offeredByPartyId, coveredByPartyId, 'partyid', 'Task_1', 'read', org, app));
  
  // Act
  var res = delegation.addMultipleRules(token, rulesList);
  
  // Assert
  var success = check(res, {
    'Add multiple rules user and org - status is 201': (r) => r.status === 201,
    'Add multiple rules user and org - rule 1 created successfully is true': (r) => r.json('0.createdSuccessfully') === true,
    'Add multiple rules user and org - rule 2 created successfully is true': (r) => r.json('1.createdSuccessfully') === true
  });
  
  addErrorCount(success);

  // Cleanup
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByUserId, 'userid', org, app);
  helper.deleteAllRules(token, performedByUserId, offeredByPartyId, coveredByPartyId, 'partyid', org, app);
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByUserId, resources, null, null);
  success = check(res, {
    'Delegate Rule To a User and Org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Delegate Rule To a User and Org - rules successfully deleted, body is empty (coveredByUserId)': (r) => r.body.includes('[]'),
  });
  policyMatchKeys.coveredBy = 'urn:altinn:partyid'
  res = delegation.getRules(token, policyMatchKeys, offeredByPartyId, coveredByPartyId, resources, null, null);
  success = check(res, {
    'Delegate Rule To a User and Org - rules successfully deleted, status is 200': (r) => r.status == 200,
    'Delegate Rule To a User and Org - rules successfully deleted, body is empty (coveredByPartyId)': (r) => r.body.includes('[]'),
  });
  addErrorCount(success);
  if(showResults == 1) {console.log('delegateRuleToAUserAndOrg:' + success)}
  sleep(3);

}


export function handleSummary(data) {
  let result = {};
  result[reportPath('authzDelegationsv2.xml')] = generateJUnitXML(data, 'platform-authorization-delegation-delegations-v2');
  return result;
}

export function showTestData() {
  console.log('environment: ' + environment);
  console.log('org1.orgno ' + org1.orgno);
  console.log('org1.partyid ' + org1.partyid);
  console.log('org2.orgno ' + org2.orgno);
  console.log('org2.partyid ' + org2.partyid);
  console.log('org1.dagl.userid ' + org1.dagl.userid);
  console.log('org1.dagl.partyid ' + org1.dagl.partyid);
  console.log('org2.dagl.userid ' + org2.dagl.userid);
  console.log('org2.dagl.partyid ' + org2.dagl.partyid);
}
