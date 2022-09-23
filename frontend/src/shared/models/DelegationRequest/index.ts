export type DelegationRequest = {
  guid: string;
  coveredBy?: string;
  coveredByName?: string;
  offeredBy?: string;
  offeredByName?: string;
  requestMessage?: string;
  created: string;
  lastChanged: string;
  requestResources?: AuthorizationRequestResource[];
  redirectUrl?: string;
  keepSessionAlive: boolean;
  requestStatus: number;
};

export type AuthorizationRequestResource = {
  serviceCode?: string;
  serviceEditionCode: number;
  altinnAppId?: string;
  operations?: string[];
  metadata?: { [key: string]: unknown };
};
