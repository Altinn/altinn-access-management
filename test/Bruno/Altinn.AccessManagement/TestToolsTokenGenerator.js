
exports.getToken = async function (getTokenParameters) {
  const axios = require("axios");
  const btoa = require("btoa");

  const tokenBaseUrl = "https://altinn-testtools-token-generator.azurewebsites.net";
  const basicAuthUser = bru.getEnvVar("tokenBasicAuthUser");
  const basicAuthPw = bru.getEnvVar("tokenBasicAuthPw");
  const Authorization = 'Basic ' + btoa(`${basicAuthUser}:${basicAuthPw}`);

  const tokenEnv = bru.getEnvVar("tokenEnv");
  const tokenType = getTokenParameters.auth_tokenType;
  const tokenScopes = getTokenParameters.auth_scopes;

  let tokenUrl;

  if (tokenType == "Personal") {
    const tokenUser = getTokenParameters.auth_userId;
    const tokenParty = getTokenParameters.auth_partyId;
    const tokenPid = getTokenParameters.auth_ssn;
    // console.log("tokenUser: " + tokenUser);
    // console.log("tokenParty: " + tokenParty);
    // console.log("tokenPid: " + tokenPid);

    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&pid=${tokenPid}&userid=${tokenUser}&partyid=${tokenParty}&authLvl=3&ttl=3000`;
  }

  else if (tokenType == "Enterprise") {
    const tokenOrg = getTokenParameters.auth_org;
    const tokenOrgNo = getTokenParameters.auth_orgNo;

    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&org=${tokenOrg}&orgNo=${tokenOrgNo}&ttl=30`;
  }

  else if (tokenType == "EnterpriseUser") {
    const tokenUser = getTokenParameters.auth_userId;
    const tokenParty = getTokenParameters.auth_partyId;
    const tokenOrgNo = getTokenParameters.auth_orgNo;
    const tokenUserName = getTokenParameters.auth_username;

    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&orgNo=${tokenOrgNo}&userId=${tokenUser}&partyId=${tokenParty}&userName=${tokenUserName}&ttl=30`;
  }

  const response = await axios.get(tokenUrl, {
    headers: { Authorization }
  });

  console.log(tokenUrl);
  // console.log(response.data);
  return response.data;
}