/*
  Test data required: deployed app (reference app: ttd/apps-test)
  userid, partyid for two users that are DAGL for two orgs, and partyid and orgno for those orgs (user1 and user2)
  Org number for user2's org
docker-compose run k6 run /src/tests/maskinporten/maskinporten.js -e env=*** -e org=*** -e app=***
-e tokengenuser=*** -e tokengenuserpwd=*** -e appsaccesskey=*** -e user1pid=*** 
-e user1userid=*** -e user1partyid=*** -e org1no=*** -e org1partyid=*** -e user2pid=*** 
-e user2userid=*** -e user2partyid=*** -e org2no=*** -e org2partyid=*** -e showresults=***

*/
import { check, sleep, fail } from 'k6';
import { addErrorCount, stopIterationOnFail } from '../../errorcounter.js';
import { generateToken } from '../../api/altinn-testtools/token-generator.js';
import { generateJUnitXML, reportPath } from '../../report.js';
import * as delegation from '../../api/platform/authorization/delegations.js';
import * as maskinporten from '../../api/platform/authorization/maskinporten.js';
import * as authorization from '../../api/platform/authorization/authorization.js';
import * as setUpData from '../../setup.js';
import * as helper from '../../Helpers/TestdataHelper.js';

let pdpInputJson = open('../../data/pdpinput.json');

const environment = __ENV.env.toLowerCase();
const tokenGeneratorUserName = __ENV.tokengenuser;
const tokenGeneratorUserPwd = __ENV.tokengenuserpwd;
const org = __ENV.org;
const app = __ENV.app;
const user1pid = __ENV.user1pid; 
const user1userid = __ENV.user1userid;
const user1partyid = __ENV.user1partyid;
const orgno1 = __ENV.org1no;
const orgpartyid1 = __ENV.org1partyid;
const user2pid = __ENV.user2pid;
const user2userid = __ENV.user2userid;
const user2partyid = __ENV.user2partyid;
const orgno2 = __ENV.org2no;
const orgpartyid2 = __ENV.org2partyid;
const showResults = __ENV.showresults;

var user1_personalToken;
var user2_personalToken;
var appOwner;
var appName;
var user1_pid;
var user1_userid;
var user1_partyid;
var org1_number;
var org1_partyid;
var user2_pid;
var user2_userid;
var user2_partyid;
var org2_number;
var org2_partyid;

export const options = {
  thresholds: {
    errors: ['count<1'],
  },
  setupTimeout: '1m',
};

export function setup() {
  //generate personal token for user 1
  var tokenGenParams = {
    env: environment,
    scopes: 'altinn:instances.read',
    pid: user1pid,
    userid: user1userid,
    partyid: user1partyid,
    authLvl: 3,
  };
  var personalToken1 = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);
  
  //generate personal token for user 2
  tokenGenParams = {
    env: environment,
    scopes: 'altinn:instances.read',
    pid: user2pid,
    userid: user2userid,
    partyid: user2partyid,
    authLvl: 3,
  };
  var personalToken2 = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  var data = {
    personalToken1: personalToken1,
    personalToken2: personalToken2,
    org: org,
    app: app,
    user1pid: user1pid,
    user1userid: user1userid,
    user1partyid: user1partyid,
    orgno1: orgno1,
    orgpartyid1: orgpartyid1,
    user2pid: user2pid,
    user2userid: user2userid,
    user2partyid: user2partyid,
    orgno2: orgno2,
    orgpartyid2: orgpartyid2,
  };

  return data;
}

//Tests for platform Authorization:Delegations:Inheritance
export default function (data) {

  // debug tokens here
  user1_personalToken = data.personalToken1;
  user2_personalToken = data.personalToken2;
  appOwner = data.org;
  appName = data.app;
  user1_pid = data.user1pid;
  user1_userid = data.user1userid;
  user1_partyid = data.user1partyid;
  org1_number = data.orgno1;
  org1_partyid = data.orgpartyid1;
  user2_pid = data.user2pid;
  user2_userid = data.user2userid;
  user2_partyid = data.user2partyid;
  org2_number = data.orgno2;
  org2_partyid = data.orgpartyid2;

  //tests
  showTestData();
  postMaskinportenSchemaToOrgNumberTest();
  postMaskinportenSchemaToPartyIdTest();
  postMaskinportenSchemaNotReadyTest();
  getMaskinPortenSchemaOfferedTest();
  getMaskinPortenSchemaReceivedTest();
  revokeOfferedMaskinPortenSchema();
  revokeReceivedMaskinPortenSchema();
  revokeNonExistentOfferedMaskinPortenSchema();
  revokeNonExistentReceivedMaskinPortenSchema();

}

/** Check that list of offered maschinportenschemas is correct */
export function getMaskinPortenSchemaOfferedTest() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;

  // Act
  var res = maskinporten.getMaskinportenSchemaOffered(offeredByToken, offeredByPartyId);
  console.log('get offered MaskinPortenSchemas');
  console.log(res.body);

  // Assert
  var success = check(res, {
    'get offered MaskinPortenSchema - status is 200': (r) => r.status === 200
  });
  addErrorCount(success);
}

/** Check that list of received maschinportenschemas is correct */
export function getMaskinPortenSchemaReceivedTest() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;

  // Act
  var res = maskinporten.getMaskinportenSchemaReceived(offeredByToken, offeredByPartyId);
  console.log('get Received MaskinPortenSchemas');
  console.log(res.body);
  
  // Assert
  var success = check(res, {
    'get Received MaskinPortenSchemas - status is 200': (r) => r.status === 200
  });
  addErrorCount(success);
}

/** offer a maskinportenschema using a party id */
export function postMaskinportenSchemaToPartyIdTest() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;
  const toPartyId = org2_partyid;
  const appid = 'appid-544';
  
  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, toPartyId, appid);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema To PartyId - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema To PartyId - to id is organizationnumber': (r) => r.json('to.0.id') === 'urn:altinn:partyid',
    'post MaskinportenSchema To PartyId - organization number matches': (r) => r.json('to.0.value') === toPartyId,
    'post MaskinportenSchema To PartyId - resource type is urn:altinn:resourceregistry': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resourceregistry',
    'post MaskinportenSchema To PartyId - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema To PartyId - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema To PartyId - action value is scopeaccess': (r) => r.json('rightDelegationResults.0.action.value') === 'scopeaccess',
  });

  addErrorCount(success);
}

/** offer a maskinportenschema using an organization number */
export function postMaskinportenSchemaToOrgNumberTest() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  const appid = 'appid-544';
  
  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, appid, 'orgno');

  // Assert
  var success = check(res, {
    'post MaskinportenSchema To Org Number - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema To Org Number - to id is organizationnumber': (r) => r.json('to.0.id') === 'urn:altinn:organizationnumber',
    'post MaskinportenSchema To Org Number - organization number matches': (r) => r.json('to.0.value') === toOrgNumber,
    'post MaskinportenSchema To Org Number - resource type is urn:altinn:resourceregistry': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resourceregistry',
    'post MaskinportenSchema To Org Number - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema To Org Number - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema To Org Number - action value is scopeaccess': (r) => r.json('rightDelegationResults.0.action.value') === 'scopeaccess',
  });

  addErrorCount(success);
}

/** try to offer a maskinportenschema that is not ready */
export function postMaskinportenSchemaNotReadyTest() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  const appid = 'appid-302';
  
  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, appid, 'orgno');

  // Assert
  var success = check(res, {
    'post MaskinportenSchema that is Not Ready - status is 400': (r) => r.status === 400,
  });

  addErrorCount(success);
}

/** revoke an offered maskinportenschema */
export function revokeOfferedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  const appid = 'appid-544';
  
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, appid, 'orgno');

  // Act
  res = maskinporten.revokeOfferedMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, appid, 'orgno');
  // console.log('revoke body: ');
  // console.log(res.body);

  // Assert
  var success = check(res, {
    'revoke Offered MaskinPortenSchema - status is 200': (r) => r.status === 200,
  });
  addErrorCount(success);
}

/** revoke a received maskinportenschema */
export function revokeReceivedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const toToken = user2_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  const appid = 'appid-544';
  
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, appid, 'orgno');

  // Act
  res = maskinporten.revokeOfferedMaskinportenSchema(toToken, offeredByPartyId, toOrgNumber, appid, 'orgno');
  // console.log('revokeReceivedMaskinPortenSchema response: ');
  // console.log(res.body);

  // Assert
  var success = check(res, {
    'revoke Received MaskinPortenSchema - status is 200': (r) => r.status === 200,
  });
  addErrorCount(success);
}

/** try to revoke a non-existent offered maskinportenschema */
export function revokeNonExistentOfferedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = user1_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  
  // Act
  var res = maskinporten.revokeOfferedMaskinportenSchema(offeredByToken, offeredByPartyId, toOrgNumber, 'nonexistent-maskinportenschema-1337', 'orgno');
  // console.log('revoke body: ');
  // console.log(res.body);

  // Assert
  var success = check(res, {
    'revoke non-existent Offered MaskinPortenSchema - status is 404': (r) => r.status === 404,
  });
  addErrorCount(success);
}

/** try to revoke a non-existent received maskinportenschema */
export function revokeNonExistentReceivedMaskinPortenSchema() {
  // Arrange
  const toToken = user2_personalToken;
  const offeredByPartyId = org1_partyid;
  const toOrgNumber = org2_number;
  
  // Act
  var res = maskinporten.revokeOfferedMaskinportenSchema(toToken, offeredByPartyId, toOrgNumber, 'nonexistent-maskinportenschema-1337', 'orgno');
  // console.log('revoke body: ');
  // console.log(res.body);

  // Assert
  var success = check(res, {
    'revoke non-existent Received MaskinPortenSchema - status is 404': (r) => r.status === 404,
  });
  addErrorCount(success);
}

export function handleSummary(data) {
  let result = {};
  result[reportPath('maskinportenschema.xml')] = generateJUnitXML(data, 'access-management-maskinportenschema');
  return result;
}

export function showTestData() {
  console.log('personalToken1: ' + user1_personalToken);
  console.log('personalToken2: ' + user2_personalToken);
  console.log('org: ' + appOwner);
  console.log('app: ' + appName);
  console.log('user1pid: ' + user1_pid);
  console.log('user1userid: ' + user1_userid);
  console.log('user1partyid: ' + user1_partyid);
  console.log('orgno1: ' + org1_number);
  console.log('orgpartyid1: ' + org1_partyid);
  console.log('user2pid: ' + user2_pid);
  console.log('user2userid: ' + user2_userid);
  console.log('user2partyid: ' + user2_partyid);
  console.log('orgno2: ' + org2_number);
  console.log('orgpartyid2: ' + org2_partyid);
}
 