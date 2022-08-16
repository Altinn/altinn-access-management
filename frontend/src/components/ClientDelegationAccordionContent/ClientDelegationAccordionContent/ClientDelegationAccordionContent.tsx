import { FC } from 'react';
import classes from './ClientDelegationAccordionContent.module.css';

export interface ClientDelegationAccordionContentProps {}

const ClientDelegationAccordionContent: FC<
  ClientDelegationAccordionContentProps
> = () => (
  <div
    data-testid="ClientDelegationAccordionContent"
    className={classes['client-delegation-accordion-content']}
  >
    ClientDelegationAccordionContent Component
  </div>
);

export default ClientDelegationAccordionContent;
