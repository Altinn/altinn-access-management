import { fetchApi } from './api';

export const getReceivedDelegationRequests = async (id: string) => {
  const response = await fetchApi(`PID${id}/DelegationRequests`);
  if (!response.ok) {
    throw new Error('Network response was not ok');
  }
  return await response.json();
};
