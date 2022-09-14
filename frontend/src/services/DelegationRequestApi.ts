import { fetchApi } from './api';

export const getReceivedDelegationRequests = async () => {
  const response = await fetchApi('PID500700/DelegationRequests');
  return await response.json();
};
