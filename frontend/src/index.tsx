import React from 'react';
import * as ReactDOM from 'react-dom';
import ReactDOMClient from 'react-dom/client';
import { QueryClient, QueryClientProvider } from 'react-query';
import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';

import FeatureAuthAdmin from './components/feature-auth-admin';
import baseLocales from './basic-locales.json';

import './index.css';

// TODO: Implement changing/saving/loading for locales
const locale = 'no';

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
    lng: locale,
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
      authAdmin: document.getElementById('altinn3-auth-admin-feature-root'),
      // further feature roots should go here…
    };

    reactRoot.render(
      <React.StrictMode>
        <QueryClientProvider client={queryClient}>
          {featureRoots.authAdmin &&
            ReactDOM.createPortal(<FeatureAuthAdmin />, featureRoots.authAdmin)}
          {/* further feature roots should be populated here… */}
        </QueryClientProvider>
      </React.StrictMode>,
    );
  },
);
