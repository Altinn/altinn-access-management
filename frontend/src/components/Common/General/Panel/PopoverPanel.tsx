import React from 'react';
import * as RadixPopover from '@radix-ui/react-popover';
import cn from 'classnames';

import type { PanelProps, RenderArrowProps } from './Panel';
import { PanelVariant } from './Panel';

import classes from './PopoverPanel.module.css';

export interface PopoverPanelProps
  extends PanelProps,
    Pick<RadixPopover.PopoverProps, 'open' | 'onOpenChange'>,
    Pick<RadixPopover.PopoverContentProps, 'side'> {
  children: React.ReactNode;
  trigger: React.ReactNode;
}

const renderArrow = ({ children }: RenderArrowProps) => {
  return (
    <RadixPopover.Arrow className={cn(classes['popover-panel__arrow'])} asChild>
      {children}
    </RadixPopover.Arrow>
  );
};

export const PopoverPanel = ({
  children,
  variant = PanelVariant.Warning,
  trigger,
  side = 'top',
  title,
  showIcon,
  forceMobileLayout,
  showPointer = true,
  onOpenChange,
  open,
}: PopoverPanelProps) => {
  return (
    <RadixPopover.Root open={open} onOpenChange={onOpenChange}>
      <RadixPopover.Trigger asChild role="button">
        {trigger}
      </RadixPopover.Trigger>
      <RadixPopover.Content side={side} className={classes['popover-panel']}>
        <div className={classes['popover-content']}>
          <span className={classes['popover-content-child']}>{children}</span>
        </div>
      </RadixPopover.Content>
    </RadixPopover.Root>
  );
};

export default PopoverPanel;
