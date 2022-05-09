import AsyncLoader from '../async-loader';
import GrantList from '../grant-list';

import { useOrganisationGrantRequests } from './api';

const OrganisationRequests = () => {
  const { isLoading, data: grantRequests } = useOrganisationGrantRequests();

  return (
    <div>
      <div>Some header</div>
      <AsyncLoader isLoading={isLoading}>
        <GrantList grants={grantRequests} />
      </AsyncLoader>
      <div>Some footer</div>
    </div>
  );
};

export default OrganisationRequests;
