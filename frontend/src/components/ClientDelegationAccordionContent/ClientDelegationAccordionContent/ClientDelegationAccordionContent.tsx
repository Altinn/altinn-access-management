import { FC } from 'react';
import classes from './ClientDelegationAccordionContent.module.css';
import TrashCanBold from '../../CompanyRequestsAccordionContent/SentCompanyRequestsHeaderAction/TrashCanBold.svg';

export interface ClientDelegationAccordionContentProps {}

const ClientDelegationAccordionContent: FC<
  ClientDelegationAccordionContentProps
> = () => (
  <div
    data-testid="ClientDelegationAccordionContent"
    className={classes['client-delegation-accordion-content']}
  >
    ClientDelegationAccordionContent Component
    <img src={TrashCanBold}></img>
  </div>
);

export default ClientDelegationAccordionContent;
