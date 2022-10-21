import React, { useState } from 'react';
import {
  Button,
  Page,
  PageHeader,
  PageContent,
} from '@altinn/altinn-design-system';
import { DelegationAccordion } from './DelegationAccordion';
import classes from './ApiDelegationAccordion.module.css';
import cn from 'classnames';

export const ApiDelegationAccordion = ({}) => {
  function unique() {
    return ++unique.i;
  }
  unique.i = 0;

  const [delegations, setDelegations] = useState([
    {
      id: unique(),
      apiName: 'Delegert API A',
      buisnesses: [
        { id: unique(), name: 'Virksomhet 1', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 2', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 3', isSoftDelete: false },
      ],
    },
    {
      id: unique(),
      apiName: 'Delegert API B',
      buisnesses: [
        { id: unique(), name: 'Virksomhet 1', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 4', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 6', isSoftDelete: false },
      ],
    },
    {
      id: unique(),
      apiName: 'Delegert API C',
      buisnesses: [
        { id: unique(), name: 'Virksomhet 1', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 4', isSoftDelete: false },
        { id: unique(), name: 'Virksomhet 5', isSoftDelete: false },
      ],
    },
  ]);

  function setBuisnessArray(
    apiID: number,
    newArray: { id: number; name: string; isSoftDelete: boolean }[],
  ) {
    const newDelegations = [...delegations];
    for (const api of newDelegations) {
      if (api.id === apiID) {
        api.buisnesses = newArray;
      }
    }
    setDelegations(newDelegations);
  }

  const isDeletableItems = () => {
    for (const api of delegations) {
      for (const item of api.buisnesses) {
        if (item.isSoftDelete) {
          return true;
        }
      }
    }
    return false;
  };

  function saveChanges() {
    const newState = [];
    for (const a of delegations) {
      const newA = {
        id: a.id,
        apiName: a.apiName,
        buisnesses: a.buisnesses.filter((b) => b.isSoftDelete === false),
      };
      if (newA.buisnesses.length > 0) {
        newState.push(newA);
      }
    }
    setDelegations(newState);
  }

  const accordions = delegations.map((i, index) => (
    <DelegationAccordion
      key={i.id}
      name={i.apiName}
      id={i.id}
      buisnesses={i.buisnesses}
      setBuisnesses={setBuisnessArray}
    ></DelegationAccordion>
  ));

  return (
    <div>
      <Page>
        <PageHeader>Header her</PageHeader>
        <PageContent>
          <div className={cn(classes['page-content'])}>
            {accordions}
            <div className={cn(classes['save-section'])}>
              <Button disabled={!isDeletableItems()} onClick={saveChanges}>
                Lagre
              </Button>
            </div>
          </div>
        </PageContent>
      </Page>
    </div>
  );
};
