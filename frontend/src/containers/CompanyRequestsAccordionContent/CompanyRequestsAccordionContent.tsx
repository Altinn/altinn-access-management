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
import { SentCompanyRequestsHeaderTexts } from '../../components/CompanyRequestsAccordionContent/SentCompanyRequestsHeaderTexts/SentCompanyRequestsHeaderTexts';
import { SentCompanyRequestsHeaderAction } from '../../components/CompanyRequestsAccordionContent/SentCompanyRequestsHeaderAction/SentCompanyRequestsHeaderAction';
import { ReceivedCompanyRequestsListItemHeader } from '../../components/CompanyRequestsAccordionContent/ReceivedCompanyRequestsListItemHeader/ReceivedCompanyRequestsListItemHeader';
import React, { useState } from 'react';
import classes from './CompanyRequestsAccordionContent.module.css';
import { useTranslation } from 'react-i18next';

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

  return (
    <div className={classes['company-requests-accordion-content']}>
      <div className={classes['company-requests-accordion-content__container']}>
        <div
          className={
            classes['company-requests-accordion-content__toggle-button-group']
          }
        >
          <ToggleButtonGroup onChange={handleChange} selectedValue={selected}>
            <ToggleButton value="received">{t('received')}</ToggleButton>
            <ToggleButton value="sent">{t('sent')}</ToggleButton>
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
                Opprett ny forespørsel
              </span>
            </Button>
          </div>
          <div className={classes['company-requests-list-container']}>
            <List>
              <ListItem>
                <ReceivedCompanyRequestsListItemHeader name="Albert Ohrem Larsen"></ReceivedCompanyRequestsListItemHeader>
              </ListItem>
              <ListItem>
                <ReceivedCompanyRequestsListItemHeader name="Morten Harket"></ReceivedCompanyRequestsListItemHeader>
              </ListItem>
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
          </div>
          <div className={classes['company-requests-accordion-container']}>
            <Accordion onClick={handleClick1} open={open1}>
              <AccordionHeader
                actions={
                  <SentCompanyRequestsHeaderAction text="Slette"></SentCompanyRequestsHeaderAction>
                }
              >
                <SentCompanyRequestsHeaderTexts
                  title="BALLANGEN OG GARDVIK"
                  subtitle="Sendt: 16.06.2022"
                ></SentCompanyRequestsHeaderTexts>
              </AccordionHeader>
              <AccordionContent>
                <div className={classes['company-requests-accordion-content']}>
                  Din forespørsel om tilganger til BALLANGEN OG GARDVIK er
                  sendt. Du vil få beskjed når de er godkjent.
                </div>
              </AccordionContent>
            </Accordion>
            <Accordion onClick={handleClick2} open={open2}>
              <AccordionHeader
                actions={
                  <SentCompanyRequestsHeaderAction text="Slette"></SentCompanyRequestsHeaderAction>
                }
              >
                <SentCompanyRequestsHeaderTexts
                  title="BARSTADT OG LEVANGER"
                  subtitle="Sendt: 13.06.2022"
                ></SentCompanyRequestsHeaderTexts>
              </AccordionHeader>
              <AccordionContent>
                <div className={classes['company-requests-accordion-content']}>
                  Din forespørsel om tilganger til BARSTADT OG LEVANGER er
                  sendt. Du vil få beskjed når de er godkjent.
                </div>
              </AccordionContent>
            </Accordion>
          </div>
        </>
      )}
      <div></div>
    </div>
  );
};
export default CompanyRequestsAccordionContent;
