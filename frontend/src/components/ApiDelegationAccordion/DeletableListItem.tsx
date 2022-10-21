import { Button, ButtonVariant, ListItem } from '@altinn/altinn-design-system';
import classes from './DeletableListItem.module.css';
import cn from 'classnames';

export interface DeletableListItemProps {
  name: string;
  isSoftDelete: boolean;
  toggleDelState: () => void;
}

export const DeletableListItem = ({
  name,
  isSoftDelete,
  toggleDelState,
}: DeletableListItemProps) => {
  return (
    <ListItem>
      <div className={classes['listItem']}>
        <div
          className={cn(classes['itemText'], {
            [classes['itemText--soft-delete']]: isSoftDelete,
          })}
        >
          {name}
        </div>
        <div className={cn(classes['deleteSection'])}>
          {isSoftDelete ? (
            <Button variant={ButtonVariant.Secondary} onClick={toggleDelState}>
              Angre
            </Button>
          ) : (
            <Button variant={ButtonVariant.Cancel} onClick={toggleDelState}>
              Slett
            </Button>
          )}
        </div>
      </div>
    </ListItem>
  );
};
