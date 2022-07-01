import React from 'react';

import classes from './AccordionHeader.module.css';
import { useAccordionContext } from './Context';
import ExpandCollapseArrow from './expand-collapse.svg';

export interface AccordionHeaderProps {
  children?: React.ReactNode;
  actions?: React.ReactNode;
}

export const AccordionHeader = ({
  children,
  actions,
}: AccordionHeaderProps) => {
  const { onClick, open } = useAccordionContext();

  return (
    <div className={classes['accordion-header']}>
      {!open ? (
        <img
          src={ExpandCollapseArrow}
          className={
            classes['accordion-header-icon']
            /*open && classes['accordion-header-icon--opened']*/
          }
          width="12"
          height="18"
          onClick={onClick}
        ></img>
      ) : (
        <img
          src={ExpandCollapseArrow}
          className={
            classes['accordion-header-icon--opened']
            /*open && classes['accordion-header-icon--opened']*/
          }
          width="12"
          height="18"
          onClick={onClick}
        ></img>
      )}
      <button
        className={classes['accordion-header__title']}
        aria-expanded={open}
        type="button"
        onClick={onClick}
      >
        {children}
      </button>
      <div className={classes['accordion-header__actions']}>{actions}</div>
    </div>
  );
};

export default AccordionHeader;
