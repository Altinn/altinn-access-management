import { useTranslation } from 'react-i18next';
import classes from './ReceivedCompanyRequestsListItemHeader.module.css';

export interface ReceivedCompanyRequestsListItemHeaderProps {
  name: string;
}

export const ReceivedCompanyRequestsListItemHeader = ({
  name,
}: ReceivedCompanyRequestsListItemHeaderProps) => {
  const { t } = useTranslation('common');
  const headerText = t('profile.access-request');

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
        {headerText}
      </span>
    </div>
  );
};
