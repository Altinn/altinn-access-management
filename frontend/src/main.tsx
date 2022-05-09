import React from 'react';
import ReactDOM from 'react-dom/client';

import OrganisationGrantRequests from './components/organisation-grant-requests';

const root = document.getElementById('altinn3-root');

if (root) {
  ReactDOM.createRoot(root).render(
    <React.StrictMode>
      <OrganisationGrantRequests />
    </React.StrictMode>,
  );
} else {
  console.warn('Cannot find root element; not starting app');
}
