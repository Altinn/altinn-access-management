import React from 'react';
import classes from './ReceivedCompanyRequestsListItemHeader.module.css';
import Warning from './Warning.svg';
import Person from './Person.svg';

export interface ReceivedCompanyRequestsListItemHeaderProps {
  name: string;
}

export const ReceivedCompanyRequestsListItemHeader = ({
  name,
}: ReceivedCompanyRequestsListItemHeaderProps) => {
  const headerText = 'Ber om tilgang';

  return (
    <div className={classes['received-company-requests-list-item-header']}>
      <img width={22} src={Person}></img>
      <span
        className={classes['received-company-requests-list-item-header__name']}
      >
        {name}
      </span>
      <img
        src={Warning}
        width={22}
        className={classes['received-company-requests-list-item-header__icon']}
      ></img>
      <span
        className={classes['received-company-requests-list-item-header__text']}
      >
        {headerText}
      </span>
    </div>
  );
};
