import { useTranslation } from 'react-i18next';

import AsyncErrorBoundary from '../AsyncErrorBoundary';
import CompanyRequestsAccordionContent from '../CompanyRequestsAccordionContent/CompanyRequestsAccordionContent';

const App = () => {
  const { t } = useTranslation('common');

  return (
    <div data-testid="FeatureAuthAdmin">
      <AsyncErrorBoundary>
        <CompanyRequestsAccordionContent />
      </AsyncErrorBoundary>
    </div>
  );
};

export default App;
