import { render, screen } from '@testing-library/react';

import ResourceRegistry from './ResourceRegistry';

describe('<ResourceRegistry />', () => {
  test('it should mount', () => {
    render(<ResourceRegistry />);

    const resourceRegistry = screen.getByTestId('ResourceRegistry');

    expect(resourceRegistry).toBeInTheDocument();
  });
});
