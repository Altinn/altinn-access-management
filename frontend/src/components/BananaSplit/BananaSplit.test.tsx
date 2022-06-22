import { render, screen } from '@testing-library/react';

import BananaSplit from './BananaSplit';

describe('<BananaSplit />', () => {
  test('it should mount', () => {
    render(<BananaSplit />);

    const bananaSplit = screen.getByTestId('BananaSplit');

    expect(bananaSplit).toBeInTheDocument();
  });
});
