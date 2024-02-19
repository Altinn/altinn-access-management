/*
  Test data required: deployed app (reference app: ttd/apps-test)
  userid, partyid for two users that are DAGL for two orgs, and partyid and orgno for those orgs (user1 and user2)
  Org number for user2's org
docker-compose run k6 run /src/tests/maskinporten/maskinporten.js -e env=*** -e tokengenuser=*** -e tokengenuserpwd=*** -e appsaccesskey=***


*/
import { check, sleep, fail } from 'k6';
import { addErrorCount, stopIterationOnFail } from '../../../errorcounter.js';
import { generateToken } from '../../../api/altinn-testtools/token-generator.js';
import { generateJUnitXML, reportPath } from '../../../report.js';
import * as maskinporten from '../../../api/access-management/maskinporten/maskinporten.js';

const environment = __ENV.env.toLowerCase();
const tokenGeneratorUserName = __ENV.tokengenuser;
const tokenGeneratorUserPwd = __ENV.tokengenuserpwd;
let testdataFile = open(`../../../data/testdata/maskinportenschema/${__ENV.env}testdata.json`);
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
    env: environment,
    scopes: 'altinn:instances.read',
    pid: testdata.org1.dagl.pid,
    userid: testdata.org1.dagl.userid,
    partyid: testdata.org1.dagl.partyid,
    authLvl: 3,
  };
  testdata.org1.dagl.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: environment,
    scopes: 'altinn:instances.read',
    pid: testdata.org2.dagl.pid,
    userid: testdata.org2.dagl.userid,
    partyid: testdata.org2.dagl.partyid,
    authLvl: 3,
  };
  testdata.org2.dagl.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: environment,
    scopes: 'altinn:instances.read',
    pid: testdata.org1.hadm.pid,
    userid: testdata.org1.hadm.userid,
    partyid: testdata.org1.hadm.partyid,
    authLvl: 3,
  };
  testdata.org1.hadm.token = generateToken('personal', tokenGeneratorUserName, tokenGeneratorUserPwd, tokenGenParams);

  tokenGenParams = {
    env: environment,
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
  postMaskinportenSchemaToOrgNumberTest();
  postMaskinportenSchemaToPartyIdTest();
  // postMaskinportenSchemaWithOrgNoInHeaderTest(); // Uncomment when #400 "post maskinportenschema with orgno in header returns 403" has been fixed
  postMaskinportenSchemaAsHadmTest();
  postMaskinportenSchemaAsApiadmTest();
  postMaskinportenCannotDelegateToPersonTest();
  postMaskinportenDAGLCannotDelegateNUFResource();
  postMaskinportenSystemResourceCannotBeDelegatedTest();
  getMaskinPortenSchemaOfferedInvalidPartyId();
  getMaskinPortenSchemaReceivedInvalidPartyId();
  postMaskinportenSchemaNotReadyTest();
  getMaskinPortenSchemaOfferedTest();
  getMaskinPortenSchemaReceivedTest();
  revokeOfferedMaskinPortenSchema();
  revokeReceivedMaskinPortenSchema();
  revokeOfferedMaskinPortenSchemaUsingPartyId();
  revokeReceivedMaskinPortenSchemaUsingPartyId();
  revokeNonExistentOfferedMaskinPortenSchema();
  revokeNonExistentReceivedMaskinPortenSchema();
  delegationCheckMaskinPortenSchema_DAGLhasScopeAccess();
  delegationCheckMaskinPortenSchema_HADMhasScopeAccess();
  delegationCheckMaskinPortenSchema_resourceDoesNotExist();
  delegationCheckMaskinPortenSchema_tooLowAuthLevel();

}

/** Check that list of offered maschinportenschemas is correct */
export function getMaskinPortenSchemaOfferedTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'ttd-am-k6-nuf';
  var res = maskinporten.revokeOfferedMaskinportenSchema(org1.dagl.token, org1.partyid, 'ttd-am-k6', 'urn:altinn:organizationnumber', org2.orgno);

  // Act
  res = maskinporten.getMaskinportenSchemaOffered(offeredByToken, offeredByPartyId);

  // Assert
  var success = check(res, {
    'get offered MaskinPortenSchemas - status is 200': (r) => r.status === 200,
    'get offered MaskinPortenSchemas - coveredByName is LARKOLLEN OG FAUSKE': (r) => r.json('0.coveredByName') === 'LARKOLLEN OG FAUSKE',
    'get offered MaskinPortenSchemas - offeredByName is LARKOLLEN OG FAUSKE': (r) => r.json('0.offeredByName') === 'ALDRA OG FORTUN',
    'get offered MaskinPortenSchemas - offeredByPartyId is ${offeredByPartyId}': (r) => r.json('0.offeredByPartyId') == offeredByPartyId,
    'get offered MaskinPortenSchemas - coveredByPartyId is ${coveredByPartyId}': (r) => r.json('0.coveredByPartyId') == org2.partyid,
    'get offered MaskinPortenSchemas - performedByUserId is ${performedByUserId}': (r) => r.json('0.performedByUserId') == org1.dagl.userid,
    'get offered MaskinPortenSchemas - offeredByOrganizationNumber is ${offeredByOrganizationNumber}': (r) => r.json('0.offeredByOrganizationNumber') == org1.orgno,
    'get offered MaskinPortenSchemas - coveredByOrganizationNumber is ${coveredByOrganizationNumber}': (r) => r.json('0.coveredByOrganizationNumber') == org2.orgno,
    'get offered MaskinPortenSchemas - resourceId is ttd-am-k6-nuf': (r) => r.json('0.resourceId') == appid,
  });
  addErrorCount(success);
}

/** Check that you can't get offered schemas using the wrong partyid */
export function getMaskinPortenSchemaOfferedInvalidPartyId() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const wrongOfferedByPartyId = org2.partyid;

  // Act
  var res = maskinporten.getMaskinportenSchemaOffered(offeredByToken, wrongOfferedByPartyId);

  // Assert
  var success = check(res, {
    'get offered MaskinPortenSchemas with invalid partyid- status is 403 forbidden': (r) => r.status === 403,
  });
  addErrorCount(success);
}

/** Check that list of received maschinportenschemas is correct */
export function getMaskinPortenSchemaReceivedTest() {
  // Arrange
  const toToken = org2.dagl.token;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6-nuf';
  var res = maskinporten.revokeOfferedMaskinportenSchema(org1.dagl.token, org1.partyid, 'ttd-am-k6', 'urn:altinn:organizationnumber', org2.orgno);

  // Act
  res = maskinporten.getMaskinportenSchemaReceived(toToken, toPartyId);

  // Assert
  var success = check(res, {
    'get Received MaskinPortenSchemas - status is 200': (r) => r.status === 200,
    'get Received MaskinPortenSchemas - offeredByName is ALDRA OG FORTUN': (r) => r.json('0.offeredByName') === 'ALDRA OG FORTUN',
    'get Received MaskinPortenSchemas - coveredByName is LARKOLLEN OG FAUSKE': (r) => r.json('0.coveredByName') === 'LARKOLLEN OG FAUSKE',
    'get Received MaskinPortenSchemas - offeredByPartyId is ${offeredByPartyId}': (r) => r.json('0.offeredByPartyId') == org1.partyid,
    'get Received MaskinPortenSchemas - coveredByPartyId is ${coveredByPartyId}': (r) => r.json('0.coveredByPartyId') == org2.partyid,
    'get Received MaskinPortenSchemas - performedByUserId is ${performedByUserId}': (r) => r.json('0.performedByUserId') == org1.dagl.userid,
    'get Received MaskinPortenSchemas - offeredByOrganizationNumber is ${offeredByOrganizationNumber}': (r) => r.json('0.offeredByOrganizationNumber') == org1.orgno,
    'get Received MaskinPortenSchemas - coveredByOrganizationNumber is ${coveredByOrganizationNumber}': (r) => r.json('0.coveredByOrganizationNumber') == org2.orgno,
    'get Received MaskinPortenSchemas - resourceId is ttd-am-k6-nuf': (r) => r.json('0.resourceId') == appid,

  });
  addErrorCount(success);
}

/** Check that you can't get received schemas using the wrong partyid */
export function getMaskinPortenSchemaReceivedInvalidPartyId() {
  // Arrange
  const toToken = org1.dagl.token;
  const wrongToPartyId = org2.partyid;

  // Act
  var res = maskinporten.getMaskinportenSchemaOffered(toToken, wrongToPartyId);

  // Assert
  var success = check(res, {
    'get received MaskinPortenSchemas with invalid partyid- status is 403 forbidden': (r) => r.status === 403,
  });
  addErrorCount(success);
}

/** offer a maskinportenschema using a party id */
export function postMaskinportenSchemaToPartyIdTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema To PartyId - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema To PartyId - to id is partyid': (r) => r.json('to.0.id') === 'urn:altinn:partyid',
    'post MaskinportenSchema To PartyId - organization number matches': (r) => r.json('to.0.value') === toPartyId,
    'post MaskinportenSchema To PartyId - resource type is urn:altinn:resource': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resource',
    'post MaskinportenSchema To PartyId - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema To PartyId - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema To PartyId - action value is ScopeAccess': (r) => r.json('rightDelegationResults.0.action.value') === 'ScopeAccess',
  });

  addErrorCount(success);
}

/** offer a maskinportenschema using an organization number */
export function postMaskinportenSchemaToOrgNumberTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toOrgNumber = org2.orgno;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:organizationnumber', toOrgNumber);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema To Org Number - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema To Org Number - to id is organizationnumber': (r) => r.json('to.0.id') === 'urn:altinn:organizationnumber',
    'post MaskinportenSchema To Org Number - organization number matches': (r) => r.json('to.0.value') === toOrgNumber,
    'post MaskinportenSchema To Org Number - resource type is urn:altinn:resource': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resource',
    'post MaskinportenSchema To Org Number - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema To Org Number - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema To Org Number - action value is ScopeAccess': (r) => r.json('rightDelegationResults.0.action.value') === 'ScopeAccess',
  });

  addErrorCount(success);
}

/** offer a maskinportenschema using the offeredby's organization number in the header */
export function postMaskinportenSchemaWithOrgNoInHeaderTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByOrganizationNumber = org1.orgno;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchemaOrgNoInHeader(offeredByToken, offeredByOrganizationNumber, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema with offeredbys orgno in header - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema with offeredbys orgno in header - to id is partyid': (r) => r.json('to.0.id') === 'urn:altinn:partyid',
    'post MaskinportenSchema with offeredbys orgno in header - organization number matches': (r) => r.json('to.0.value') === toPartyId,
    'post MaskinportenSchema with offeredbys orgno in header - resource type is urn:altinn:resource': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resource',
    'post MaskinportenSchema with offeredbys orgno in header - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema with offeredbys orgno in header - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema with offeredbys orgno in header - action value is ScopeAccess': (r) => r.json('rightDelegationResults.0.action.value') === 'ScopeAccess',
  });

  addErrorCount(success);
}

/** offer a maskinportenschema as HADM (instead of DAGL) */
export function postMaskinportenSchemaAsHadmTest() {
  // Arrange
  const offeredByToken = org1.hadm.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema As HADM - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema As HADM - to id is partyid': (r) => r.json('to.0.id') === 'urn:altinn:partyid',
    'post MaskinportenSchema As HADM - organization number matches': (r) => r.json('to.0.value') === toPartyId,
    'post MaskinportenSchema As HADM - resource type is urn:altinn:resource': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resource',
    'post MaskinportenSchema As HADM - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema As HADM - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema As HADM - action value is ScopeAccess': (r) => r.json('rightDelegationResults.0.action.value') === 'ScopeAccess',
  });

  addErrorCount(success);
}

/** offer a maskinportenschema as APIADM (instead of DAGL) */
export function postMaskinportenSchemaAsApiadmTest() {
  // Arrange
  const offeredByToken = org1.apiadm.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema As APIADM - status is 201': (r) => r.status === 201,
    'post MaskinportenSchema As APIADM - to id is partyid': (r) => r.json('to.0.id') === 'urn:altinn:partyid',
    'post MaskinportenSchema As APIADM - organization number matches': (r) => r.json('to.0.value') === toPartyId,
    'post MaskinportenSchema As APIADM - resource type is urn:altinn:resource': (r) => r.json('rightDelegationResults.0.resource.0.id') === 'urn:altinn:resource',
    'post MaskinportenSchema As APIADM - appid matches': (r) => r.json('rightDelegationResults.0.resource.0.value') === appid,
    'post MaskinportenSchema As APIADM - action type is action-id': (r) => r.json('rightDelegationResults.0.action.id') === 'urn:oasis:names:tc:xacml:1.0:action:action-id',
    'post MaskinportenSchema As APIADM - action value is ScopeAccess': (r) => r.json('rightDelegationResults.0.action.value') === 'ScopeAccess',
  });

  addErrorCount(success);
}

/** try and fail to delegate maskinportenschema to a person */
export function postMaskinportenCannotDelegateToPersonTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.dagl.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema Cannot Delegate To Person - status is 400': (r) => r.status === 400,
    'post MaskinportenSchema Cannot Delegate To Person - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'post MaskinportenSchema Cannot Delegate To Person - errors is not null': (r) => r.json('errors') != null,
    'post MaskinportenSchema Cannot Delegate To Person - Invalid organization': (r) => r.body.includes('Maskinporten schema delegation can only be delegated to a valid organization'),
  });

  addErrorCount(success);
}

/** try and fail to delegate maskinportenschema to a person */
export function postMaskinportenDAGLCannotDelegateNUFResource() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6-nuf';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);
  console.log(res.body);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema DAGL Cannot Delegate NUF Resource - status is 400': (r) => r.status === 400,
    'post MaskinportenSchema DAGL Cannot Delegate NUF Resource - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'post MaskinportenSchema DAGL Cannot Delegate NUF Resource - errors is not null': (r) => r.json('errors') != null,
    'post MaskinportenSchema DAGL Cannot Delegate NUF Resource - Unauthorized to delegate the resource': (r) => r.body.includes('Authenticated user does not have any delegable rights for the resource: ttd-am-k6-nuf'),
  });

  addErrorCount(success);
}

/** try and fail to delegate a system resource */
export function postMaskinportenSystemResourceCannotBeDelegatedTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'altinn_maskinporten_scope_delegation';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'post Maskinporten Systemresource Cannot be delegated - status is 400': (r) => r.status === 400,
    'post Maskinporten Systemresource Cannot be delegated - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'post Maskinporten Systemresource Cannot be delegated - errors is not null': (r) => r.json('errors') != null,
    'post Maskinporten Systemresource Cannot be delegated - Invalid resource type': (r) => r.body.includes('This operation only support requests for Maskinporten schema resources. Invalid resource: altinn_maskinporten_scope_delegation. Invalid resource type: SystemResource"'),
  });

  addErrorCount(success);
}

/** try to offer a maskinportenschema that is not ready */
export function postMaskinportenSchemaNotReadyTest() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toOrgNumber = org2.orgno;
  const appid = 'appid-302';

  // Act
  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:organizationnumber', toOrgNumber);

  // Assert
  var success = check(res, {
    'post MaskinportenSchema that is Not Ready - status is 400': (r) => r.status === 400,
    'post MaskinportenSchema that is Not Ready - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'post MaskinportenSchema that is Not Ready - errors is not null': (r) => r.json('errors') != null,
    'post MaskinportenSchema that is Not Ready - resource is incomplete or not found': (r) => r.body.includes('The resource: appid-302, does not exist or is not complete and available for delegation'),
  });

  addErrorCount(success);
}

/** revoke an offered maskinportenschema */
export function revokeOfferedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toOrgNumber = org2.orgno;
  const appid = 'ttd-am-k6';

  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:organizationnumber', toOrgNumber);
  success = check(res, {
    'revoke Offered MaskinPortenSchema - getMaskinPortenSchema was added (status is 201 created)': (r) => r.status === 201,
  });
  addErrorCount(success);

  // Act
  res = maskinporten.revokeOfferedMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:organizationnumber', toOrgNumber);

  // Assert
  var success = check(res, {
    'revoke Offered MaskinPortenSchema - revoke status is 204': (r) => r.status === 204,
  });
  addErrorCount(success);

  res = maskinporten.getMaskinportenSchemaOffered(offeredByToken, offeredByPartyId);
  success = check(res, {
    'revoke Offered MaskinPortenSchema - getMaskinPortenSchema returns only 1 element': (r) => r.json('1') == null,
  });
  addErrorCount(success);
}

/** revoke a received maskinportenschema */
export function revokeReceivedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const toToken = org2.dagl.token;
  const offeredByPartyId = org1.partyid;
  const offeredByOrgNumber = org1.orgno;
  const toOrgNumber = org2.orgno;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:organizationnumber', toOrgNumber);
  success = check(res, {
    'revoke Received MaskinPortenSchema - getMaskinPortenSchema was added (status is 201 created)': (r) => r.status === 201,
  });
  addErrorCount(success);

  // Act
  res = maskinporten.revokeReceivedMaskinportenSchema(toToken, toPartyId, appid, 'urn:altinn:organizationnumber', offeredByOrgNumber);

  // Assert
  var success = check(res, {
    'revoke Received MaskinPortenSchema - status is 204': (r) => r.status === 204,
  });
  addErrorCount(success);

  res = maskinporten.getMaskinportenSchemaReceived(toToken, toPartyId);
  success = check(res, {
    'revoke Received MaskinPortenSchema - getMaskinPortenSchema returns only 1 element': (r) => r.json('1') == null,
  });
  addErrorCount(success);
}

/** revoke an offered maskinportenschema using partyid*/
export function revokeOfferedMaskinPortenSchemaUsingPartyId() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);
  success = check(res, {
    'revoke Offered MaskinPortenSchema using partyid - getMaskinPortenSchema was added (status is 201 created)': (r) => r.status === 201,
  });
  addErrorCount(success);

  // Act
  res = maskinporten.revokeOfferedMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);

  // Assert
  var success = check(res, {
    'revoke Offered MaskinPortenSchema using partyid- revoke status is 204': (r) => r.status === 204,
  });
  addErrorCount(success);

  res = maskinporten.getMaskinportenSchemaOffered(offeredByToken, offeredByPartyId);
  success = check(res, {
    'revoke Offered MaskinPortenSchema using partyid- getMaskinPortenSchema returns only 1 element': (r) => r.json('1') == null,
  });
  addErrorCount(success);
}

/** revoke a received maskinportenschema using partyid*/
export function revokeReceivedMaskinPortenSchemaUsingPartyId() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const toToken = org2.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toPartyId = org2.partyid;
  const appid = 'ttd-am-k6';

  var res = maskinporten.postMaskinportenSchema(offeredByToken, offeredByPartyId, appid, 'urn:altinn:partyid', toPartyId);
  success = check(res, {
    'revoke Received MaskinPortenSchema using partyid - getMaskinPortenSchema was added (status is 201 created)': (r) => r.status === 201,
  });
  addErrorCount(success);

  // Act
  res = maskinporten.revokeReceivedMaskinportenSchema(toToken, toPartyId, appid, 'urn:altinn:partyid', offeredByPartyId);

  // Assert
  var success = check(res, {
    'revoke Received MaskinPortenSchema using partyid - status is 204': (r) => r.status === 204,
  });
  addErrorCount(success);

  sleep(3);
  res = maskinporten.getMaskinportenSchemaReceived(toToken, toPartyId);
  success = check(res, {
    'revoke Received MaskinPortenSchema using partyid - getMaskinPortenSchema returns only 1 element': (r) => r.json('1') == null,
  });
  addErrorCount(success);
}

/** try to revoke a non-existent offered maskinportenschema */
export function revokeNonExistentOfferedMaskinPortenSchema() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const toOrgNumber = org2.orgno;

  // Act
  var res = maskinporten.revokeOfferedMaskinportenSchema(offeredByToken, offeredByPartyId, 'nonexistentmaskinportenschema', 'urn:altinn:organizationnumber', toOrgNumber);

  // Assert
  var success = check(res, {
    'revoke non-existent Offered MaskinPortenSchema - status is 400': (r) => r.status === 400,
    'revoke non-existent Offered MaskinPortenSchema - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'revoke non-existent Offered MaskinPortenSchema - errors is not null': (r) => r.json('errors') != null,
    'revoke non-existent Offered MaskinPortenSchema - resource is incomplete or not found': (r) => r.body.includes('The resource: nonexistentmaskinportenschema, does not exist or is not complete and available for delegation'),
  });
  addErrorCount(success);
}

/** try to revoke a non-existent received maskinportenschema */
export function revokeNonExistentReceivedMaskinPortenSchema() {
  // Arrange
  const toToken = org2.dagl.token;
  const toPartyId = org2.partyid;
  const offeredByOrgNumber = org1.orgno;

  // Act
  var res = maskinporten.revokeReceivedMaskinportenSchema(toToken, toPartyId, 'nonexistentmaskinportenschema', 'urn:altinn:organizationnumber', offeredByOrgNumber);

  // Assert
  var success = check(res, {
    'revoke non-existent Received MaskinPortenSchema - status is 400': (r) => r.status === 400,
    'revoke non-existent Received MaskinPortenSchema - `One or more validation errors occurred.`': (r) => r.json('title') == 'One or more validation errors occurred.',
    'revoke non-existent Received MaskinPortenSchema - errors is not null': (r) => r.json('errors') != null,
    'revoke non-existent Received MaskinPortenSchema - resource is incomplete or not found': (r) => r.body.includes('The resource: nonexistentmaskinportenschema, does not exist or is not complete and available for delegation'),
  });
  addErrorCount(success);
}

/** check delegation rights on a maskinportenschema for DAGL role */
export function delegationCheckMaskinPortenSchema_DAGLhasScopeAccess() {
  // Arrange
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postDelegationCheck(offeredByToken, offeredByPartyId, appid);

  // Assert
  var success = check(res, {
    'delegationCheckMaskinportenSchema: DAGL has scope access - status is 200': (r) => r.status === 200,
    'delegationCheckMaskinportenSchema: DAGL has scope access - rightskey is ttd-am-k6:ScopeAccess': (r) => r.json('0.rightKey') === appid + ':ScopeAccess',
    'delegationCheckMaskinportenSchema: DAGL has scope access - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
    'delegationCheckMaskinportenSchema: DAGL has scope access - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
    'delegationCheckMaskinportenSchema: DAGL has scope access - action value is ScopeAccess': (r) => r.json('0.action') == 'ScopeAccess',
    'delegationCheckMaskinportenSchema: DAGL has scope access - status is Delegable': (r) => r.json('0.status') == 'Delegable',
    'delegationCheckMaskinportenSchema: DAGL has scope access - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
    'delegationCheckMaskinportenSchema: DAGL has scope access - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
    'delegationCheckMaskinportenSchema: DAGL has scope access - RoleRequirementsMatches value is APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'APIADM',
  });
  addErrorCount(success);
}

/** check delegation rights on a maskinportenschema for HADM role*/
export function delegationCheckMaskinPortenSchema_HADMhasScopeAccess() {
  // Arrange
  const offeredByToken = org1.hadm.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'ttd-am-k6';

  // Act
  var res = maskinporten.postDelegationCheck(offeredByToken, offeredByPartyId, appid);

  // Assert
  var success = check(res, {
    'delegationCheckMaskinportenSchema: HADM has scope access - status is 200': (r) => r.status === 200,
    'delegationCheckMaskinportenSchema: HADM has scope access - rightskey is ttd-am-k6:ScopeAccess': (r) => r.json('0.rightKey') === appid + ':ScopeAccess',
    'delegationCheckMaskinportenSchema: HADM has scope access - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
    'delegationCheckMaskinportenSchema: HADM has scope access - resource value is ttd-am-k6': (r) => r.json('0.resource.0.value') == appid,
    'delegationCheckMaskinportenSchema: HADM has scope access - action value is ScopeAccess': (r) => r.json('0.action') == 'ScopeAccess',
    'delegationCheckMaskinportenSchema: HADM has scope access - status is Delegable': (r) => r.json('0.status') == 'Delegable',
    'delegationCheckMaskinportenSchema: HADM has scope access - code is RoleAccess': (r) => r.json('0.details.0.code') == 'RoleAccess',
    'delegationCheckMaskinportenSchema: HADM has scope access - RoleRequirementsMatches id is urn:altinn:rolecode': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.id') == 'urn:altinn:rolecode',
    'delegationCheckMaskinportenSchema: HADM has scope access - RoleRequirementsMatches value is APIADM': (r) => r.json('0.details.0.parameters.RoleRequirementsMatches.0.value') == 'APIADM',
  });
  addErrorCount(success);
}

/** Checks that the response is 400 and with an error message when trying to delegate a non-existing resource */
export function delegationCheckMaskinPortenSchema_resourceDoesNotExist() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'aresourcethattotallydoesnotexist';
  
  // Act
  var res = maskinporten.postDelegationCheck(offeredByToken, offeredByPartyId, appid);

  // Assert
  var success = check(res, {
    'delegationCheckMaskinportenSchema: Resource Is NonDelegable - status is 400 ': (r) => r.status === 400,
    'delegationCheckMaskinportenSchema: Resource Is NonDelegable - Error message is "The resource does not exist or is not available for delegation"': (r) => r.body.includes('The resource does not exist or is not available for delegation'),
  });
  addErrorCount(success);
}

/** Checks that the response is 400 and with an error message when trying to delegate with a too low authentication level */
export function delegationCheckMaskinPortenSchema_tooLowAuthLevel() {
  const offeredByToken = org1.dagl.token;
  const offeredByPartyId = org1.partyid;
  const appid = 'altinn_automation_test_lv4';
  
  // Act
  var res = maskinporten.postDelegationCheck(offeredByToken, offeredByPartyId, appid);

  // Assert
  var success = check(res, {
    'delegationCheckMaskinportenSchema: user has too low authentication level - status is 200': (r) => r.status === 200,
    'delegationCheckMaskinportenSchema: user has too low authentication level - rightskey is altinn_automation_test_lv4:ScopeAccess': (r) => r.json('0.rightKey') === appid + ':ScopeAccess',
    'delegationCheckMaskinportenSchema: user has too low authentication level - resource id is urn:altinn:resource': (r) => r.json('0.resource.0.id') === 'urn:altinn:resource',
    'delegationCheckMaskinportenSchema: user has too low authentication level - resource value is altinn_automation_test_lv4': (r) => r.json('0.resource.0.value') == appid,
    'delegationCheckMaskinportenSchema: user has too low authentication level - action value is ScopeAccess': (r) => r.json('0.action') == 'ScopeAccess',
    'delegationCheckMaskinportenSchema: user has too low authentication level - status is NotDelegable': (r) => r.json('0.status') == 'NotDelegable',
    'delegationCheckMaskinportenSchema: user has too low authentication level - code is InsufficientAuthenticationLevel': (r) => r.json('0.details.0.code') == 'InsufficientAuthenticationLevel',
    'delegationCheckMaskinportenSchema: user has too low authentication level - MinimumAuthenticationLevel id is urn:altinn:minimum-authenticationlevel': (r) => r.json('0.details.0.parameters.MinimumAuthenticationLevel.0.id') == 'urn:altinn:minimum-authenticationlevel',
    'delegationCheckMaskinportenSchema: user has too low authentication level - MinimumAuthenticationLevel value is 4': (r) => r.json('0.details.0.parameters.MinimumAuthenticationLevel.0.value') == '4',
  });
  addErrorCount(success);
}


export function handleSummary(data) {
  let result = {};
  result[reportPath('maskinportenschema.xml')] = generateJUnitXML(data, 'access-management-maskinportenschema');
  return result;
}

export function showTestdata() {
  console.log(environment)
  console.log('personalToken1: ' + org1.dagl.token);
  console.log('personalToken2: ' + org2.dagl.token);
  console.log('org: ' + testdata.org);
  console.log('app: ' + testdata.app);
  console.log('user1pid: ' + org1.dagl.pid);
  console.log('user1userid: ' + org1.dagl.userid);
  console.log('user1partyid: ' + org1.dagl.partyid);
  console.log('orgno1: ' + org1.orgno);
  console.log('orgpartyid1: ' + org1.partyid);
  console.log('hadm1userid: ' + org1.hadm.userid);
  console.log('hadm1partyid: ' + org1.hadm.partyid);
  console.log('user2pid: ' + org2.dagl.pid);
  console.log('user2userid: ' + org2.dagl.userid);
  console.log('user2partyid: ' + org2.dagl.partyid);
  console.log('orgno2: ' + org2.orgno);
  console.log('orgpartyid2: ' + org2.partyid);
}
