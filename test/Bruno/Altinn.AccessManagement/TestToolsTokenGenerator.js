
exports.getToken = async function (tokenType, scopes, tokenPid, tokenUser, tokenParty) {
  const axios = require("axios");
  const btoa = require("btoa");

  const tokenBaseUrl = "https://altinn-testtools-token-generator.azurewebsites.net";
  const basicAuthUser = bru.getEnvVar("tokenBasicAuthUser");
  const basicAuthPw = bru.getEnvVar("tokenBasicAuthPw");
  const Authorization = 'Basic ' + btoa(`${basicAuthUser}:${basicAuthPw}`);

  const tokenEnv = bru.getEnvVar("tokenEnv");
  // const tokenType = bru.getVar("auth_tokenType");
  // const tokenUser = auth_userId;
  // const tokenParty = bru.getVar("auth_partyId");
  // const tokenPid = bru.getVar("auth_ssn");
  // const tokenScopes = scopes;
  const tokenOrg = bru.getVar("auth_org");
  const tokenOrgNo = bru.getVar("auth_orgNo");
  const tokenUsername = bru.getVar("auth_username");

  console.log("tokenGenerator scopes: " + scopes);
  console.log("tokenGenerator tokenPid: " + tokenPid);
  console.log("tokenGenerator tokenUser: " + tokenUser);
  console.log("tokenGenerator tokenParty: " + tokenParty);
  
  let tokenUrl;
  if (tokenType == "Personal") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${scopes}&pid=${tokenPid}&userid=${tokenUser}&partyid=${tokenParty}&authLvl=3&ttl=3000`;
  }

  if (tokenType == "Enterprise") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${scopes}&org=${tokenOrg}&orgNo=${tokenOrgNo}&ttl=30`;
  }

  if (tokenType == "EnterpriseUser") {
    tokenUrl = `${tokenBaseUrl}/api/Get${tokenType}Token?env=${tokenEnv}&scopes=${scopes}&orgNo=${tokenOrgNo}&userId=${tokenUser}&partyId=${tokenParty}&userName=${tokenUserName}&ttl=30`;
  }

  // console.log("tokenUrl:" +  tokenUrl);

  const response = await axios.get(tokenUrl, {
    headers: { Authorization }
  });

  return response.data;
}