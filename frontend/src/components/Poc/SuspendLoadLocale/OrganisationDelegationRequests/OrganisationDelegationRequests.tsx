import { Panel } from '@altinn/altinn-design-system';
import { useQuery } from 'react-query';
import { fetchApi } from '../../../../services/api';

import DelegationList from '../DelegationList';
import { DelegationRequest } from '../DelegationList/models';

const DelegationRequests = () => {
  const { data: delegationRequests } = useQuery<DelegationRequest[]>(
    'DelegationRequests',
    async () => {
      const response = await fetchApi('PID500700/DelegationRequests');
      return await response.json();
    },
    { suspense: true, staleTime: 10000 },
  );

  return (
    <div>
      <div>Some header</div>
      <DelegationList delegations={delegationRequests} />
      <Panel title="This is a…">Panel!</Panel>
      <div>Some footer</div>
    </div>
  );
};

export default DelegationRequests;
