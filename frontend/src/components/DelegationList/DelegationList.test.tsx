import { render, screen } from '@testing-library/react';

import DelegationList from './DelegationList';

describe('<DelegationList />', () => {
  test('it should mount', () => {
    render(<DelegationList />);

    const delegationList = screen.getByTestId('DelegationList');

    expect(delegationList).toBeInTheDocument();
  });

  test('it should mount', () => {
    render(<DelegationList />);

    const delegationList = screen.getByTestId('DelegationList');

    expect(delegationList).toBeInTheDocument();
  });
});
