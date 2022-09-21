import { List, ListItem } from '@altinn/altinn-design-system';
import { PersonListItem } from '../../common/PersonListItem/PersonListItem';
import { useTranslation } from 'react-i18next';
import { DelegationRequest } from '../../../shared/models/DelegationRequest';
import { getReceivedDelegationRequests } from '../../../services/DelegationRequestApi';
import { useQuery } from 'react-query';

export const ReceivedCompanyRequestsAccordionContent = () => {
  const { t } = useTranslation('common');

  const { data: requests } = useQuery<DelegationRequest[]>(
    'DelegationRequests',
    async () => getReceivedDelegationRequests('500700'),
    {
      suspense: true,
      staleTime: 10000,
    },
  );

  return (
    <List>
      {requests?.map((request) => {
        return (
          <ListItem key={request.guid}>
            <PersonListItem
              name={request.coveredByName}
              rightText={t('profile.access_request')}
            ></PersonListItem>
          </ListItem>
        );
      })}
    </List>
  );
};
