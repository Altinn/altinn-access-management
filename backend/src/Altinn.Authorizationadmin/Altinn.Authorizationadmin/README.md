# Back end for Altinn 3 Authorization

## Environment variables

These environment variables control where the front end is loaded from:

- `FRONTEND_MODE`
  - if set to `Development`, the FE will be loaded from a development server (e.g. started with Yarn). This allows the FE to perform hot reloading during development
  - otherwise, the FE is loaded from static (prebuilt) files

- `FRONTEND_DEV_URL`
  - if `FRONTEND_MODE` is `Development`, this provides the base URL for the FE development server, usually running in the same machine (default: `http://localhost:3000`)

- `FRONTEND_PROD_FOLDER`
  - if `FRONTEND_MODE` is **not** `Development`, this it the relative path to where prebuilt files are served from (default: `wwwroot/AuthorizationAdmin`)
