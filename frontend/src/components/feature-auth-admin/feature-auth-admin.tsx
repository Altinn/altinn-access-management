import { useTranslation } from 'react-i18next';

import AsyncErrorBoundary from '../async-error-boundary';
import OrganisationDelegationRequests from '../organisation-delegation-requests';

const App = () => {
  const { t } = useTranslation('common');

  return (
    <>
      <h1>{t('greeting')}…</h1>
      <AsyncErrorBoundary>
        <OrganisationDelegationRequests />
      </AsyncErrorBoundary>
    </>
  );
};

export default App;
