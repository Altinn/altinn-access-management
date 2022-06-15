import { DropFirst } from '../utils/typing';
import { getConfig } from './config';

export const getApiPath = (path: string) => {
  return new URL(path, getConfig('backendApiUrl'));
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
