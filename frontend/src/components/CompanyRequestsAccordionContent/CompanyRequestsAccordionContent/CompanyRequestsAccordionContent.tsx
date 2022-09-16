import {
  ToggleButton,
  ToggleButtonGroup,
  Button,
  ButtonVariant,
  Accordion,
  AccordionContent,
  AccordionHeader,
  List,
  ListItem,
} from '@altinn/altinn-design-system';
import { SentCompanyRequestsHeaderTexts } from '../SentCompanyRequestsHeaderTexts/SentCompanyRequestsHeaderTexts';
import { PersonListItem } from '../../Common/PersonListItem/PersonListItem';
import { useState } from 'react';
import classes from './CompanyRequestsAccordionContent.module.css';
import { useTranslation } from 'react-i18next';
import { DelegationRequest } from '../../../shared/models/DelegationRequest';
import { getReceivedDelegationRequests } from '../../../services/DelegationRequestApi';
import { useQuery } from 'react-query';
import { fetchApi } from '../../../services/api';

export interface ChangeProps {
  selectedValue: string;
}

const CompanyRequestsAccordionContent = () => {
  const { t } = useTranslation('common');
  const [selected, setSelected] = useState('received');
  const [open1, setOpen1] = useState(false);
  const [open2, setOpen2] = useState(false);

  const handleClick1 = () => {
    setOpen1(!open1);
  };

  const handleClick2 = () => {
    setOpen2(!open2);
  };

  const handleChange = ({ selectedValue }: ChangeProps) => {
    setSelected(selectedValue);
  };

  const { data: requests } = useQuery<DelegationRequest[]>(
    'DelegationRequests',
    async () => getReceivedDelegationRequests('500700'),
    {
      suspense: true,
      staleTime: 10000,
    },
  );

  console.log(requests);

  return (
    <div className={classes['company-requests-accordion-content']}>
      <div className={classes['company-requests-accordion-content__container']}>
        <div
          className={
            classes['company-requests-accordion-content__toggle-button-group']
          }
        >
          <ToggleButtonGroup onChange={handleChange} selectedValue={selected}>
            <ToggleButton value="received">
              {t('profile.received')}
            </ToggleButton>
            <ToggleButton value="sent">{t('profile.sent')}</ToggleButton>
          </ToggleButtonGroup>
        </div>
      </div>
      {selected === 'received' ? (
        <div>
          <div
            className={classes['company-requests-accordion-content__container']}
          >
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
                  classes[
                    'company-requests-accordion-content__span--button-text'
                  ]
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
      ) : (
        <>
          <div
            className={classes['company-requests-accordion-content__container']}
          >
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
                  classes[
                    'company-requests-accordion-content__span--button-text'
                  ]
                }
              >
                {t('profile.create_request')}
              </span>
            </Button>
          </div>
          <div
            className={
              classes[
                'company-requests-accordion-content__sent-requests-accordion'
              ]
            }
          >
            <Accordion onClick={handleClick1} open={open1}>
              {requests?.map((request) => {
                return (
                  <>
                    <AccordionHeader>
                      <SentCompanyRequestsHeaderTexts
                        title={request.offeredByName}
                        subtitle={request.created}
                      ></SentCompanyRequestsHeaderTexts>
                    </AccordionHeader>
                    <AccordionContent>
                      <div
                        className={
                          classes[
                            'company-requests-accordion-content__sent-requests-accordion-content'
                          ]
                        }
                      >
                        Din forespørsel om tilganger til {request.offeredByName}
                        er sendt. Du vil få beskjed når de er godkjent.
                      </div>
                    </AccordionContent>
                  </>
                );
              })}
            </Accordion>
          </div>
        </>
      )}
      <div></div>
    </div>
  );
};
export default CompanyRequestsAccordionContent;
