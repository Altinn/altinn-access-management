import React from 'react';
import ReactDOM from 'react-dom/client';

import OrganisationRequests from './components/organisation-requests';

const root = document.getElementById('altinn3-root');

if (root) {
  ReactDOM.createRoot(root).render(
    <React.StrictMode>
      <OrganisationRequests />
    </React.StrictMode>,
  );
} else {
  console.warn('Cannot find root element; not starting app');
}
