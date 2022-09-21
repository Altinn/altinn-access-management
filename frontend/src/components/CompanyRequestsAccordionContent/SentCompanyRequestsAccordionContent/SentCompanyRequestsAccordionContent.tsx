import {
  Button,
  ButtonVariant,
  Accordion,
  AccordionContent,
  AccordionHeader,
} from '@altinn/altinn-design-system';
import classes from './SentCompanyRequestsAccordionContent.module.css';
import { useTranslation } from 'react-i18next';
import { DelegationRequest } from '../../../shared/models/DelegationRequest';
import { getReceivedDelegationRequests } from '../../../services/DelegationRequestApi';
import { useQuery } from 'react-query';
import { SentCompanyRequestsHeaderTexts } from '../SentCompanyRequestsHeaderTexts/SentCompanyRequestsHeaderTexts';
import { useState } from 'react';

export const SentCompanyRequestsAccordionContent = () => {
  const { t } = useTranslation('common');
  const [open1, setOpen1] = useState(false);

  const handleClick1 = () => {
    setOpen1(!open1);
  };

  const { data: requests } = useQuery<DelegationRequest[]>(
    'DelegationRequests',
    async () => getReceivedDelegationRequests('500700'),
    {
      suspense: true,
      staleTime: 10000,
    },
  );
  return (
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
  );
};

export default SentCompanyRequestsAccordionContent;
