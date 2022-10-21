import React, { useState } from 'react';
import {
  Accordion,
  AccordionHeader,
  AccordionContent,
  Button,
  ButtonVariant,
  List,
  BorderStyle,
} from '@altinn/altinn-design-system';
import { DeletableListItem } from './DeletableListItem';
import classes from './DelegationAccordion.module.css';
import cn from 'classnames';

export interface DeletableListItemProps {
  name: string;
  id: number;
  buisnesses: {
    id: number;
    name: string;
    isSoftDelete: boolean;
  }[];
  setBuisnesses: (
    id: number,
    newArray: {
      id: number;
      name: string;
      isSoftDelete: boolean;
    }[],
  ) => void;
}

export const DelegationAccordion = ({
  name,
  id,
  buisnesses,
  setBuisnesses,
}: DeletableListItemProps) => {
  const [open, setOpen] = useState(false);

  const handleAccordionClick = () => {
    setOpen(!open);
  };

  function toggleBuisnessState(id: number) {
    const newArray = [...buisnesses];
    for (const item of newArray) {
      if (item.id === id) {
        item.isSoftDelete = !item.isSoftDelete;
      }
    }
    setBuisnesses(id, newArray);
  }

  const buisnessItems = buisnesses.map((b, index) => (
    <DeletableListItem
      key={index + b.id}
      name={b.name}
      isSoftDelete={b.isSoftDelete}
      toggleDelState={() => toggleBuisnessState(b.id)}
    ></DeletableListItem>
  ));

  const isAllSoftDeleted = () => {
    for (const item of buisnesses) {
      if (!item.isSoftDelete) {
        return false;
      }
    }
    return true;
  };

  function softDeleteAll() {
    const newArray = [...buisnesses];
    for (const item of newArray) {
      item.isSoftDelete = true;
    }
    setBuisnesses(id, newArray);
  }

  function reinstateAll() {
    const newArray = [...buisnesses];
    for (const item of newArray) {
      item.isSoftDelete = false;
    }
    setBuisnesses(id, newArray);
  }

  function handleSoftDeleteAll() {
    softDeleteAll();
    setOpen(true);
  }

  const action = isAllSoftDeleted() ? (
    <Button variant={ButtonVariant.Secondary} onClick={reinstateAll}>
      Angre
    </Button>
  ) : (
    <Button variant={ButtonVariant.Cancel} onClick={handleSoftDeleteAll}>
      Slett
    </Button>
  );

  return (
    <Accordion onClick={handleAccordionClick} open={open}>
      <AccordionHeader
        subtitle={buisnesses.length + ' virksomheter har tilgang'}
        actions={action}
      >
        <div
          className={cn({
            [classes['accordion-header--soft-delete']]: isAllSoftDeleted(),
          })}
        >
          {name}
        </div>
      </AccordionHeader>
      <AccordionContent>
        <div className={cn(classes['accordion-content'])}>
          <List borderStyle={BorderStyle.Dashed}>{buisnessItems}</List>
        </div>
      </AccordionContent>
    </Accordion>
  );
};
