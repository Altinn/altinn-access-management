import { DropFirst } from '../utils/typing';

const API_FALLBACK = new URL(window.location.href).origin + '/api/';

// You can define the ALTINN_APP_API in a `/.env.local` file, which won't be committed to git
// @see https://vitejs.dev/guide/env-and-mode.html#env-files

export const getApiPath = (path: string) => {
  return new URL(path, import.meta.env.VITE_ALTINN_APP_API ?? API_FALLBACK);
};

export const fetchApi = (
  input: RequestInfo,
  ...extraArgs: DropFirst<Parameters<typeof fetch>>
) => {
  let request: Request;

  if (typeof input === 'string') {
    request = new Request(getApiPath(input).href);
  } else {
    request = new Request({ ...input, url: getApiPath(input.url).href });
  }

  return fetch(request, ...extraArgs);
};
