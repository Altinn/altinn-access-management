import { QueryClient, QueryClientProvider, useQuery } from 'react-query';

export function useOrganisationGrantRequests() {
  return useQuery('repoData', () =>
    fetch('https://api.github.com/repos/tannerlinsley/react-query').then(
      (res) => res.json(),
    ),
  );
}
