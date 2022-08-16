import { render, screen } from '@testing-library/react';

import ClientDelegationAccordionContent from '.';

describe('<ClientDelegationAccordionContent />', () => {
  test('it should mount', () => {
    render(<ClientDelegationAccordionContent />);

    const clientDelegationAccordionContent = screen.getByTestId('ClientDelegationAccordionContent');

    expect(clientDelegationAccordionContent).toBeInTheDocument();
  });
});
