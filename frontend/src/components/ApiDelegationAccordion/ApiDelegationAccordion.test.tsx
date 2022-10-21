import { render, screen } from '@testing-library/react';

import { ApiDelegationAccordion } from './ApiDelegationAccordion';

describe('<ApiDelegationAccordion />', () => {
  test('it should mount', () => {
    render(<ApiDelegationAccordion />);

    const apiDelegationAccordion = screen.getByTestId('ApiDelegationAccordion');

    expect(apiDelegationAccordion).toBeInTheDocument();
  });
});
