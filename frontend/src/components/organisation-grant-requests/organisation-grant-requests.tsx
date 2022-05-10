import { useQuery } from 'react-query';

import GrantList from '../grant-list';
import { Grant } from '../grant-list/models';

const OrganisationRequests = () => {
  const { data: grantRequests } = useQuery(
    'OrganisationRequests',
    async () => {
      return await new Promise<Grant[]>((r, x) => {
        setTimeout(() => {
          x('oh noes sad');
        }, 0);
      });
      // const data = await fetch(
      //   'https://api.github.com/repos/tannerlinsley/react-query',
      // );
      // return await data.json();
    },
    { suspense: true },
  );

  return (
    <div>
      <div>Some header</div>
      <GrantList grants={grantRequests} />
      <div>Some footer</div>
    </div>
  );
};

export default OrganisationRequests;
