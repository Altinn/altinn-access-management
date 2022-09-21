import {
  Button,
  ButtonVariant,
  ToggleButton,
  ToggleButtonGroup,
} from '@altinn/altinn-design-system';
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

  const toggleAccordionContent = (value: string) => {
    switch (value) {
      case 'received':
        return <ReceivedCompanyRequestsAccordionContent />;
      case 'sent':
        return <SentCompanyRequestsAccordionContent />;
    }
  };

  return (
    <div className={classes['company-requests-accordion-content']}>
      <>
        <div
          className={classes['company-requests-accordion-content__container']}
        >
          <div
            className={
              classes['company-requests-accordion-content__toggle-button-group']
            }
          >
            <ToggleButtonGroup onChange={handleChange} selectedValue={selected}>
              <ToggleButton value={'received'}>
                {t('profile.received')}
              </ToggleButton>
              <ToggleButton value={'sent'}>{t('profile.sent')}</ToggleButton>
            </ToggleButtonGroup>
          </div>
        </div>
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
                classes['company-requests-accordion-content__span--button-text']
              }
            >
              {t('profile.create_request')}
            </span>
          </Button>
        </div>
        <div className={classes['company-requests-list-container']}>
          {toggleAccordionContent(selected)}
        </div>
      </>
    </div>
  );
};
export default CompanyRequestsAccordionContent;
