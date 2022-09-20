import { ToggleButton, ToggleButtonGroup } from '@altinn/altinn-design-system';
import { useState } from 'react';
import classes from './CompanyRequestsAccordionContent.module.css';
import { useTranslation } from 'react-i18next';
import { ReceivedCompanyRequestsAccordionContent } from '../ReceivedCompanyRequestsAccordionContent';
import { SentCompanyRequestsAccordionContent } from '../SentCompanyRequestsAccordionContent';

export interface ChangeProps {
  selectedValue: string;
}

export enum CompanyRequestToggles {
  Received = 'received',
  Sent = 'sent',
}

const CompanyRequestsAccordionContent = () => {
  const { t } = useTranslation('common');
  const [selected, setSelected] = useState('received');

  const handleChange = ({ selectedValue }: ChangeProps) => {
    setSelected(selectedValue);
  };

  return (
    <div className={classes['company-requests-accordion-content']}>
      <div className={classes['company-requests-accordion-content__container']}>
        <div
          className={
            classes['company-requests-accordion-content__toggle-button-group']
          }
        >
          <ToggleButtonGroup onChange={handleChange} selectedValue={selected}>
            <ToggleButton value={CompanyRequestToggles.Received}>
              {t('profile.received')}
            </ToggleButton>
            <ToggleButton value={CompanyRequestToggles.Sent}>
              {t('profile.sent')}
            </ToggleButton>
          </ToggleButtonGroup>
        </div>
      </div>
      switch(selected) {
        case CompanyRequestToggles.Received: return (
          
        )

      } === CompanyRequestToggles.Received ? (
        <ReceivedCompanyRequestsAccordionContent />
      ) : (
        <SentCompanyRequestsAccordionContent />
      )}
    </div>
  );
};
export default CompanyRequestsAccordionContent;
