# altinn-access-management

This component will handle backend functionality related to Access Management
- Administration of rights for apps, resources
- Administration of rights for api schemes

## Getting started

The fastest way to get development going is to open the main solution Altinn.AccessManagement.sln and selecting 'Altinn.AccessManagement' as the start up project from Visual Studio. Browser should open automatically to the swagger ui for the API.

Alternatively:

- Start the backend in `/src/Altinn.AccessManagement/Altinn.AccessManagement` with `dotnet run` or `dotnet watch`

## Project organisation

This is a typical backend API solution written in .NET C#.

- The main back end project is in `/src/Altinn.Authorizationadmin` and it has [its own README](backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/README.md)

- There is also a "bridge" between the back end and older APIs. For local development, this is implemented in `/development/src/LocalBridge`


## Setting up database

To run Access Management locally you need to have PostgreSQL database installed

- Download [PostgreSQL](https://www.postgresql.org/download/) (Currently using 14 in Azure, but 15 works locally) 
- Install database server (choose your own admin password and save it some place you can find it again)
- Start PG admin


Create database authorizationdb

Create the following users (with priveliges for authorizationdb) 
-platform_authorization_admin (superuser, canlogin)
-platform_authorization (canlogin)
password: Password

Create schema delegations in authorizationdb

Set platform_authorization_admin as owner



