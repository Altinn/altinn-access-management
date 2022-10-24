# altinn-access-management

This component will handle backend functionality related to Access Management
- Administration of rights for apps, resources
- Administration of rights for api schemes

## Getting started

The fastest way to get development going is to open the main solution Altinn.AccessManagement.sln and selecting 'Altinn.AccessManagement' as the start up project from Visual Studio. Browser should open automatically to the swagger ui for the API.

Alternatively:

- Start the backend in `/src/Altinn.AccessManagement/Altinn.AccessManagement` with `dotnet run` or `dotnet watch`

  ```sh
  VITE_ALTINN_APP_API=http://localhost:5117/api/
  ```

## Project organisation

This is a typical backend API solution written in .NET C#.

- The main back end project is in `/src/Altinn.Authorizationadmin` and it has [its own README](backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/README.md)

- There is also a "bridge" between the back end and older APIs. For local development, this is implemented in `/development/src/LocalBridge`

