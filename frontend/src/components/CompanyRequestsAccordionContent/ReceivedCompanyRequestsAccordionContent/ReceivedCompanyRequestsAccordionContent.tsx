import {
  Button,
  ButtonVariant,
  List,
  ListItem,
} from '@altinn/altinn-design-system';
import { PersonListItem } from '../../common/PersonListItem/PersonListItem';
import classes from './ReceivedCompanyRequestsAccordionContent.module.css';
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
    <div>
      <div className={classes['company-requests-accordion-content__container']}>
        <Button
          variant={ButtonVariant.Primary}
          className={
            classes['company-requests-accordion-content__button--request']
          }
          data-action="load"
          data-target="#altinnModal"
          data-toggle="altinn-modal"
          data-url="/ui/DelegationRequest?modalOnly=true"
        >
          <span
            className={
              classes['company-requests-accordion-content__span--button-text']
            }
          >
            {t('profile.create_request')}
          </span>
        </Button>
      </div>
      <div className={classes['company-requests-list-container']}>
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
      </div>
    </div>
  );
};
