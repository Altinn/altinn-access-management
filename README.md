# altinn-authorization-admin

PoC for new auth admin UI

## Getting started

The fastest way to get development going is to run `docker compose up`. Both the backend and the frontend will start in watch mode; you should be able to edit files and see changes live on the browser. The frontend will be available on http://localhost:3000, and the backend on http://localhost:5117

Alternatively:

- Start the proxy bridge in `development/src/LocalBridge` with `dotnet run` or `dotnet watch`
- Start the backend in `backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin` with `dotnet run` or `dotnet watch`
- Start the frontend in `frontend` with `yarn install` and `yarn dev --host`
