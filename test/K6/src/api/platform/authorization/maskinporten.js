import http from 'k6/http';
import * as config from '../../../config.js';
import * as header from '../../../buildrequestheaders.js';

/**
 * GET call to get maskinportenschemas that have been offered by the current party
 * @param {*} altinnToken personal token for offering party's DAGL 
 * @param {*} partyid the offering party's partyid or organization number
 */
export function getMaskinportenSchemaOffered(altinnToken, partyid) {
  var endpoint = config.buildMaskinPorteSchemaUrls(partyid, 'offered');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  console.log(`endpoint: ${endpoint}`);
  return http.get(endpoint, params);
}

/**
 * GET call to get maskinportenschemas that have been received by the current party
 * @param {*} altinnToken personal token for the receiving party's DAGL
 * @param {*} partyid the receiving party's partyid or organization number
 */
export function getMaskinportenSchemaReceived(altinnToken, partyid) {
  var endpoint = config.buildMaskinPorteSchemaUrls(partyid, 'received');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  console.log(`endpoint: ${endpoint}`);
  return http.get(endpoint, params);
}

/**
 * POST call to revoke maskinportenschemas that have been offered by the current party
 * @param {*} altinnToken personal token for DAGL of offeredby
 * @param {*} offeredByPartyId the offering party's partyid
 * @param {*} to the receiving organization's party id or organization number
 * @param {*} resourceid the id of the resource to delegate
 * @param {*} orgnoOrPartyId 'orgno' to set id to urn:altinn:organizationnumber
 */
export function revokeOfferedMaskinportenSchema(altinnToken, offeredByPartyId, to, resourceid, orgnoOrPartyId) {
  var endpoint = config.buildMaskinPorteSchemaUrls(offeredByPartyId, 'revokeoffered');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  console.log(`endpoint: ${endpoint}`);
  var body = [];
  body.push(generatePolicyMatch(to, resourceid, orgnoOrPartyId));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1)
  console.log('revokeReceivedMaskinportenSchema request:');
  console.log(bodystring);
  return http.post(endpoint, bodystring, params);
}

/**
 * POST call to revoke maskinportenschemas that have been received by the current party
 * @param {*} altinnToken personal token for DAGL of receiver
 * @param {*} offeredByPartyId the offering party's partyid
 * @param {*} to the receiving organization's party id or organization number
 * @param {*} resourceid the id of the resource to delegate
 * @param {*} orgnoOrPartyId 'orgno' to set id to urn:altinn:organizationnumber
 */
export function revokeReceivedMaskinportenSchema(altinnToken, partyid) {
  var endpoint = config.buildMaskinPorteSchemaUrls(partyid, 'revokereceived');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  console.log(`endpoint: ${endpoint}`);
  var body = [];
  body.push(generatePolicyMatch(to, resourceid, orgnoOrPartyId, 'from'));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1);
  console.log('revokeReceivedMaskinportenSchema request:');
  console.log(bodystring);
  return http.post(endpoint, bodystring, params);
}

/**
 * GET call to get maskinportenschemas that have been received by the current party
 * @param {*} altinnToken personal token for DAGL
 * @param {*} toPartyId party id or organization number of whom that offers the rule
 * @param {*} to the receiving organization's party id or organization number
 * @param {*} resourceid the id of the resource to delegate
 * @param {*} orgnoOrPartyId 'orgno' to set id to urn:altinn:organizationnumber
 */
export function postMaskinportenSchema(altinnToken, offeredByPartyId, to, resourceid, orgnoOrPartyId) {
  var endpoint = config.buildMaskinPorteSchemaUrls(offeredByPartyId, 'maskinportenschema');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  var body = [];
  body.push(generatePolicyMatch(to, resourceid, orgnoOrPartyId));
  console.log(`endpoint: ${endpoint}`);
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1)
  console.log('postMaskinportenSchema body: ');
  console.log(bodystring);
  return http.post(endpoint, bodystring, params);
}

/**
 * function to build a policy match with action, user and resource details
 * @param {*} toPartyId party id or organization number of whom that receives the rule
 * @param {*} altinnAction read,write,sign
 * @param {*} orgnoOrPartyId 'orgno' to set id to urn:altinn:organizationnumber
 * @param {*} toOrFrom 'to', 'from'. Decides whether to use "to" or "from" in the JSon
 * @returns json object of a completed policy match
 */
function generatePolicyMatch(toPartyId, resourceid, orgnoOrPartyId, toOrFrom) {
  var toId = 'urn:altinn:partyid';
  if (orgnoOrPartyId == 'orgno') {
    toId = 'urn:altinn:organizationnumber'
  }
  var toOrFrom = {
    id: toId,
    value: toPartyId
  }
  var rights = {
    resource: [{
      id: 'urn:altinn:resourceregistry',
      value: resourceid
    }],
  }
  var policyMatch = {};
  
  if(toOrFrom == 'from') {
    policyMatch = {
      from: [],
      rights: []
    };
    policyMatch.from.push(toOrFrom);
  }
  else {
    policyMatch = {
      to: [],
      rights: []
    };
    policyMatch.to.push(toOrFrom);
  }
  policyMatch.rights.push(rights);
  return policyMatch;
}
