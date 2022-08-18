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
import { useState } from 'react';
import classes from './ClientDelegationAccordionContent.module.css';
import { useTranslation } from 'react-i18next';
import { PersonListItem } from '../../Common/PersonListItem/PersonListItem';

export interface ChangeProps {
  selectedValue: string;
}

const ClientDelegationAccordionContent = () => {
  const { t } = useTranslation('common');
  const [selected, setSelected] = useState('employees');
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
            <ToggleButton value="employees">
              {t('profile.employees')}
            </ToggleButton>
            <ToggleButton value="clients">{t('profile.clients')}</ToggleButton>
          </ToggleButtonGroup>
        </div>
      </div>
      {selected === 'employees' ? (
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
              data-url="/ui/ClientDelegation/AddNewClientRoleHolder?modalOnly=true"
            >
              <span
                className={
                  classes[
                    'company-requests-accordion-content__span--button-text'
                  ]
                }
              >
                {t('profile.add_new_employee')}
              </span>
            </Button>
          </div>
          <div className={classes['company-requests-list-container']}>
            <List>
              <ListItem>
                <PersonListItem
                  name="Albert Ohrem Larsen"
                  rightText={t('profile.add_new_employee')}
                ></PersonListItem>
              </ListItem>
              <ListItem>
                <PersonListItem
                  name="Marion Dragland"
                  rightText={t('profile.add_new_employee')}
                ></PersonListItem>
              </ListItem>
            </List>
          </div>
        </div>
      ) : (
        <>
          <div
            className={classes['company-requests-accordion-content__container']}
          ></div>
          <div
            className={
              classes[
                'company-requests-accordion-content__sent-requests-accordion'
              ]
            }
          >
            <List>
              <ListItem>
                <PersonListItem
                  name="Albert Ohrem Larsen"
                  rightText={t('profile.change_rights')}
                ></PersonListItem>
              </ListItem>
              <ListItem>
                <PersonListItem
                  name="Marion Dragland"
                  rightText={t('profile.change_rights')}
                ></PersonListItem>
              </ListItem>
            </List>
          </div>
        </>
      )}
      <div></div>
    </div>
  );
};
export default ClientDelegationAccordionContent;
