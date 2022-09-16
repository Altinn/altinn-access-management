import React from 'react';
import ReactDOMClient from 'react-dom/client';
import i18next from 'i18next';
import * as ReactDOM from 'react-dom';
import { QueryClient, QueryClientProvider } from 'react-query';
import { initReactI18next } from 'react-i18next';

import SuspendLoadLocale from './helpers/SuspendLoadLocale';

import { getConfig } from './services/config';
import baseLocales from './basic-locales.json';
import './index.css';
import CompanyRequestsAccordionContent from './components/CompanyRequestsAccordionContent/CompanyRequestsAccordionContent';

/**
 * Special behaviour for react-query in dev environment
 */
const queryClientDevDefaults = {
  queries: {
    retry: false, // Don't retry failed requests
  },
};

// Initialise i18next; start application when ready
i18next.use(initReactI18next).init(
  {
    lng: getConfig('defaultLocale'),
    ns: ['common', 'basic'],
    defaultNS: 'common',
    resources: baseLocales,
  },

  () => {
    // Configure react-query
    const queryClient = new QueryClient({
      defaultOptions: import.meta.env.DEV ? queryClientDevDefaults : undefined,
    });

    // Create a div to host the React application
    const rootEl = document.createElement('div');
    rootEl.id = 'altinn3-app-root';
    document.body.appendChild(rootEl);
    const reactRoot = ReactDOMClient.createRoot(rootEl);

    // Get references to the DOM nodes that will serve as portal for features
    // Note that there must be a <div id="xyz"> in HTML that matches the IDs below

    const featureRoots = {
      authAdmin: document.getElementById(
        'altinn3-company-requests-accordion-content',
      ),
      // further feature roots should go here…
    };

    reactRoot.render(
      <React.StrictMode>
        <QueryClientProvider client={queryClient}>
          <SuspendLoadLocale>
            {featureRoots.authAdmin &&
              ReactDOM.createPortal(
                <CompanyRequestsAccordionContent />,
                featureRoots.authAdmin,
              )}
          </SuspendLoadLocale>
        </QueryClientProvider>
      </React.StrictMode>,
    );
  },
);
