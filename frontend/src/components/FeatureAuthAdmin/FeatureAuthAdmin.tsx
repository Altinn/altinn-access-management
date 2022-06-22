import { useTranslation } from 'react-i18next';

import AsyncErrorBoundary from '../AsyncErrorBoundary';
import OrganisationDelegationRequests from '../OrganisationDelegationRequests';

const App = () => {
  const { t } = useTranslation('common');

  return (
    <div data-testid="FeatureAuthAdmin">
      <h1>{t('greeting')}…</h1>
      <AsyncErrorBoundary>
        <OrganisationDelegationRequests />
      </AsyncErrorBoundary>
    </div>
  );
};

export default App;
