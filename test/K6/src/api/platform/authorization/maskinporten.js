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
export function revokeOfferedMaskinportenSchema(altinnToken, offeredByPartyId, resourceid, attributeId, attributeValue) {
  var endpoint = config.buildMaskinPorteSchemaUrls(offeredByPartyId, 'revokeoffered');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  var body = [];
  body.push(makeRequestBody(resourceid, attributeId, attributeValue));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1)
  return http.post(endpoint, bodystring, params);
}

/**
 * POST call to revoke maskinportenschemas that have been received by the current party
 * @param {*} altinnToken personal token for DAGL of receiver
 * @param {*} coveredByPartyId the receiving party's partyid
 * @param {*} resourceid the id of the resource to delegate
 * @param {*} attributeId the attribute id for the offerer of the schema. 'urn:altinn:partyid' or 'urn:altinn:organizationnumber'
 * @param {*} attributeValue the offerer's partyid or organization number
 */
export function revokeReceivedMaskinportenSchema(altinnToken, coveredByPartyId, resourceid, attributeId, attributeValue) {
  var endpoint = config.buildMaskinPorteSchemaUrls(coveredByPartyId, 'revokereceived');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  var body = [];
  body.push(makeRequestBody(resourceid, null, null, attributeId, attributeValue));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1);
  return http.post(endpoint, bodystring, params);
}

/**
 * GET call to get maskinportenschemas that have been received by the current party
 * @param {*} altinnToken personal token for DAGL
 * @param {*} toPartyId party id or organization number of whom that offers the rule
 * @param {*} resourceid the id of the resource to delegate
 * @param {*} attributeId the attribute id for the receiver of the schema. 'urn:altinn:partyid' or 'urn:altinn:organizationnumber'
 * @param {*} attributeValue the receiver's partyid or organization number
 */
export function postMaskinportenSchema(altinnToken, offeredByPartyId, resourceid, attributeId, attributeValue) {
  var endpoint = config.buildMaskinPorteSchemaUrls(offeredByPartyId, 'maskinportenschema');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken, 'personal');
  var body = [];
  body.push(makeRequestBody(resourceid, attributeId, attributeValue));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1)
  return http.post(endpoint, bodystring, params);
}

/**
 * function to build the request body for maskinportenschema
 * @param {*} resourceid the resourceid for the schema
 * @param {*} toAttributeId the attribute id for the receiver of the schema. 'urn:altinn:partyid' or 'urn:altinn:organizationnumber'
 * @param {*} toAttributeValue the receiver's partyid or organization number
 * @param {*} fromAttributeId the attribute id for the sender of the schema. 'urn:altinn:partyid' or 'urn:altinn:organizationnumber'
 * @param {*} fromAttributeValue the sender's partyid or organization number
 * @returns request body json object
 */
function makeRequestBody(resourceid, toAttributeId, toAttributeValue, fromAttributeId, fromAttributeValue) {
  var rights = {
    resource: [{
      id: 'urn:altinn:resource',
      value: resourceid
    }],
  }
  var requestBody = {};
  
  if (toAttributeId != null && toAttributeValue != null) {
    requestBody = {
      to: [],
      rights: []
    };
    requestBody.to.push({id: toAttributeId, value: toAttributeValue});
  }

  else {
    requestBody = {
      from: [],
      rights: []
    };
    requestBody.from.push({id: fromAttributeId, value: fromAttributeValue});
  }

  requestBody.rights.push(rights);
  return requestBody;
}