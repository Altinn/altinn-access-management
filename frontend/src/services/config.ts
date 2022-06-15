/**
 * Application configuration is obtained from three possible sources:
 * - fallback configuration (inline in this file)
 * - a JSON config object in a `<script type="application/json" id="altinn3-app-config">` element in the DOM
 * - VITE_* environment variables
 */

type Config = {
  backendApiUrl?: string;
  // other configurable keys here…
};

// --- Fallback configuration (if nothing else loads) ------------------------

const fallbackConfig: Config = {
  backendApiUrl: new URL(window.location.href).origin + '/api/',
  // other configurable keys here…
};

// --- JSON configuration (from HTML) ----------------------------------------

const configElId = 'altinn3-app-config';
const configEl = document.getElementById(configElId);

let jsonConfig: Config = {};

if (!configEl) {
  console.error(
    `Could not load configuration; element with ID" ${configElId}" not found`,
  );
} else {
  try {
    jsonConfig = JSON.parse(configEl.innerText);
  } catch (e) {
    console.error('Could not parse configuration');
  }
}

// --- Environment configuration (from variables) ----------------------------

// You can define the VITE_* variables in a `/.env.local` file, which won't be committed to git
// @see https://vitejs.dev/guide/env-and-mode.html#env-files

const envConfig: Config = {
  backendApiUrl: import.meta.env.VITE_BACKEND_API_URL,
  // other configurable keys here…
};

for (const key in envConfig) {
  if (envConfig[key as keyof Config] === undefined) {
    delete envConfig[key as keyof Config];
  }
}

// ---------------------------------------------------------------------------

// Apply configuration (env vars override JSON, which overrides fallback)

const config: Config = { ...fallbackConfig, ...jsonConfig, ...envConfig };

export function getConfig(key: keyof Config) {
  return config[key];
}
