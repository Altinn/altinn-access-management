# altinn-authorization-admin

PoC for new auth admin UI

## Getting started

The fastest way to get development going is to run `docker compose up`. Both the backend and the frontend will start in watch mode; you should be able to edit files and see changes live on the browser. The frontend will be available on http://localhost:3000, and the backend on http://localhost:5117

Alternatively:

- Start the proxy bridge in `/development/src/LocalBridge` with `dotnet run` or `dotnet watch`
- Start the backend in `/backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin` with `dotnet run` or `dotnet watch`
- Create a `.env.local` file in `/frontend` which defines where to find the backend:

  ```sh
  VITE_ALTINN_APP_API=http://localhost:5117/api/
  ```

- Start the frontend in `/frontend` with `yarn install` and `yarn dev`

## Project organisation

This is a typical "back-end/front-end" solution, with the back end written in .NET C#, while the front end is a React app using Vite.

- The main back end project is in `/backend/src/Altinn.Authorizationadmin` and it has [its own README](backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/README.md)

- There is also a "bridge" between the back end and older APIs. For local development, this is implemented in `/development/src/LocalBridge`

- The front end is in `/frontend` and it also has [its own README](frontend/README.md)