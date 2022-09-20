import { render, screen } from '@testing-library/react';

import SentCompanyRequestsAccordionContent from './SentCompanyRequestsAccordionContent';

describe('<SentCompanyRequestsAccordionContent />', () => {
  test('it should mount', () => {
    render(<SentCompanyRequestsAccordionContent />);

    const sentCompanyRequestsAccordionContent = screen.getByTestId('SentCompanyRequestsAccordionContent');

    expect(sentCompanyRequestsAccordionContent).toBeInTheDocument();
  });
});
