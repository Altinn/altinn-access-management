import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from 'react-query';
import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';

import App from './components/app/app';
import AsyncErrorBoundary from './components/async-error-boundary';
import baseLocales from './basic-locales.json';

import './main.css';

const locale = 'no';

/**
 * Special behaviour for react-query in dev environment
 */
const queryClientDevDefaults = {
  queries: {
    retry: false, // Don't retry failed requests
  },
};

i18next.use(initReactI18next).init(
  {
    lng: locale,
    debug: true,
    ns: ['common', 'basic'],
    defaultNS: 'common',
    resources: baseLocales,
  },

  () => {
    const root = document.getElementById('altinn3-root');

    if (root) {
      const queryClient = new QueryClient({
        defaultOptions: import.meta.env.DEV
          ? queryClientDevDefaults
          : undefined,
      });

      ReactDOM.createRoot(root).render(
        <React.StrictMode>
          <QueryClientProvider client={queryClient}>
            <AsyncErrorBoundary>
              <App />
            </AsyncErrorBoundary>
          </QueryClientProvider>
        </React.StrictMode>,
      );
    } else {
      console.warn('Cannot find root element; not starting app');
    }
  },
);
