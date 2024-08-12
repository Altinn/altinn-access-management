
exports.getToken = async function (scopes) {
  const axios = require("axios");
  const btoa = require("btoa");

  const tokenBaseUrl = "https://altinn-testtools-token-generator.azurewebsites.net";
  const basicAuthUser = bru.getEnvVar("tokenBasicAuthUser");
  const basicAuthPw = bru.getEnvVar("tokenBasicAuthPw");
  const Authorization = 'Basic ' + btoa(`${basicAuthUser}:${basicAuthPw}`);

  const tokenEnv = bru.getEnvVar("tokenEnv");
  const tokenType = bru.getVar("auth_tokenType");
  const tokenUser = bru.getVar("auth_userId");
  const tokenParty = bru.getVar("auth_partyId");
  const tokenPid = bru.getVar("auth_ssn");
  const tokenScopes = scopes;
  const tokenOrg = bru.getVar("auth_org");
  const tokenOrgNo = bru.getVar("auth_orgNo");
  const tokenUsername = bru.getVar("auth_username");

  let tokenUrl;
  if (tokenType == "Personal") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&pid=${tokenPid}&userid=${tokenUser}&partyid=${tokenParty}&authLvl=3&ttl=3000`;
  }

  if (tokenType == "Enterprise") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&org=${tokenOrg}&orgNo=${tokenOrgNo}&ttl=30`;
  }

  if (tokenType == "EnterpriseUser") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${tokenScopes}&orgNo=${tokenOrgNo}&userId=${tokenUser}&partyId=${tokenParty}&userName=${tokenUserName}&ttl=30`;
  }

  // console.log("tokenUrl:" +  tokenUrl);

  const response = await axios.get(tokenUrl, {
    headers: { Authorization }
  });

  return response.data;
}