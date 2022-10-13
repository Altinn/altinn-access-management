FROM mcr.microsoft.com/dotnet/sdk:6.0.401-alpine3.16 AS build
WORKDIR /app


COPY backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/*.csproj ./src/Altinn.Authorizationadmin/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Core/*.csproj ./src/Altinn.AuthorizationAdmin.Core/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Integration/*.csproj ./src/Altinn.AuthorizationAdmin.Core/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Persistance/*.csproj ./src/Altinn.AuthorizationAdmin.Persistance/
RUN dotnet restore ./src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.csproj


# Copy everything else and build
COPY backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/ ./src/Altinn.Authorizationadmin/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Core/ ./src/Altinn.AuthorizationAdmin.Core/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Integration/ ./src/Altinn.AuthorizationAdmin.Core/
COPY backend/src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.Persistance/ ./src/Altinn.AuthorizationAdmin.Persistance/
RUN dotnet publish -c Release -o out ./src/Altinn.Authorizationadmin/Altinn.AuthorizationAdmin.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-alpine3.16 AS final
EXPOSE 5100
WORKDIR /app
COPY --from=build /app/out .

COPY backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/Migration ./Migration

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry
ENTRYPOINT ["dotnet", "Altinn.Altinn.AuthorizationAdmin.dll"]
