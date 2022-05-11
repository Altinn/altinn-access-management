import i18next from 'i18next';
import { useTranslation } from 'react-i18next';
import { useQuery } from 'react-query';

import AsyncErrorBoundary from '../async-error-boundary';
import OrganisationGrantRequests from '../organisation-grant-requests';

const App = () => {
  const { t, i18n } = useTranslation('common');

  useQuery(
    'Locales',
    async () => {
      const data = await fetch(`locales/${i18n.language}.json`);
      const resource = await data.json();
      i18next.addResourceBundle(i18n.language, 'common', resource);
    },
    {
      staleTime: Infinity,
      suspense: true,
    },
  );

  return (
    <>
      <h1>{t('greeting')}…</h1>
      <AsyncErrorBoundary>
        <OrganisationGrantRequests />
      </AsyncErrorBoundary>
    </>
  );
};

export default App;
