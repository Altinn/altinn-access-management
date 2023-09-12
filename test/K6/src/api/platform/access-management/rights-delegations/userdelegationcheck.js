import http from 'k6/http';
import * as config from '../../../../config.js';
import * as header from '../../../../buildrequestheaders.js';

export function resourceUserDelegationCheck(altinnToken, offeredByPartyId, resourceid) {
    var endpoint = config.buildRightsEndpointUrls(offeredByPartyId, 'userdelegationcheck');
    var params = header.buildHeaderWithRuntimeAndJson(altinnToken);
    var body = [];
    body.push(makeResourceRequestBody(resourceid));
    var bodystring = JSON.stringify(body);
    bodystring = bodystring.substring(1, bodystring.length-1)
    return http.post(endpoint, bodystring, params);
}

function makeResourceRequestBody(resourceid) {
    var requestBody = {
      resource: [{
        id: 'urn:altinn:resource',
        value: resourceid
      }],
    };
  
    return requestBody;
}

export function appUserDelegationCheck(altinnToken, offeredByPartyId, org, app) {
  var endpoint = config.buildRightsEndpointUrls(offeredByPartyId, 'userdelegationcheck');
  var params = header.buildHeaderWithRuntimeAndJson(altinnToken);
  var body = [];
  body.push(makeAppRequestBody(org, app));
  var bodystring = JSON.stringify(body);
  bodystring = bodystring.substring(1, bodystring.length-1)
  return http.post(endpoint, bodystring, params);
}

function makeAppRequestBody(org, app) {
  var requestBody = {
    resource: [{
      id: 'urn:altinn:org',
      value: org
    },
    {
      id: 'urn:altinn:app',
      value: app
    }
  ],
  };

  return requestBody;
}