import i18next from 'i18next';
import { useTranslation } from 'react-i18next';
import { useQuery } from 'react-query';

import AsyncErrorBoundary from '../async-error-boundary';
import OrganisationDelegationRequests from '../organisation-delegation-requests';

const App = () => {
  const { t, i18n } = useTranslation('common');
  const baseUrl = import.meta.env.BASE_URL;
  const localeFilePath = `${baseUrl}locales/${i18n.language}.json`;
  const localeFileUrl = new URL(localeFilePath, import.meta.url).href;

  useQuery(
    'Locales',
    async () => {
      const data = await fetch(localeFileUrl);
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
        <OrganisationDelegationRequests />
      </AsyncErrorBoundary>
    </>
  );
};

export default App;
