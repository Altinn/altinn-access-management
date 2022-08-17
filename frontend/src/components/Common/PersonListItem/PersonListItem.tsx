import { useTranslation } from 'react-i18next';
import classes from './PersonListItem.module.css';

export interface PersonListItemProps {
  name: string;
  rightText: string;
}

export const PersonListItem = ({ name, rightText }: PersonListItemProps) => {
  const { t } = useTranslation('common');

  return (
    <div className={classes['received-company-requests-list-item-header']}>
      <span
        className={classes['received-company-requests-list-item-header__name']}
      >
        {name}
      </span>
      <span
        className={classes['received-company-requests-list-item-header__text']}
      >
        {rightText}
      </span>
    </div>
  );
};
