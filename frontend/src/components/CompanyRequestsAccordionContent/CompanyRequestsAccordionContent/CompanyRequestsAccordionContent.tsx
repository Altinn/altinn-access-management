import {
  ToggleButton,
  ToggleButtonGroup,
  Button,
  ButtonVariant,
} from '@altinn/altinn-design-system';
import { SentCompanyRequestsHeaderTexts } from '../SentCompanyRequestsHeaderTexts/SentCompanyRequestsHeaderTexts';
import { SentCompanyRequestsHeaderAction } from '../SentCompanyRequestsHeaderAction/SentCompanyRequestsHeaderAction';
import React, { useState } from 'react';
import classes from './CompanyRequestsAccordionContent.module.css';

export interface ChangeProps {
  selectedValue: string;
}

const CompanyRequestsAccordionContent = () => {
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
            <ToggleButton value="received">Mottatte</ToggleButton>
            <ToggleButton value="sent">Sendte</ToggleButton>
          </ToggleButtonGroup>
        </div>
        {selected === 'received' ? (
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
              Opprett ny forespørsel
            </span>
          </Button>
        ) : (
          <>
            <Button
              variant={ButtonVariant.Primary}
              className={
                classes['company-requests-accordion-content__button--request']
              }
              data-action=""
              data-target=""
              data-toggle=""
              data-url=""
            >
              <span
                className={
                  classes[
                    'company-requests-accordion-content__span--button-text'
                  ]
                }
              >
                Opprett ny forespørsel
              </span>
            </Button>
            <div>
              <SentCompanyRequestsHeaderTexts
                title="Tittel "
                subtitle="Undertittel"
              ></SentCompanyRequestsHeaderTexts>
              <SentCompanyRequestsHeaderAction text="Slette"></SentCompanyRequestsHeaderAction>
            </div>
          </>
        )}
        <div></div>
      </div>
    </div>
  );
};
export default CompanyRequestsAccordionContent;
