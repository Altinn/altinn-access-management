FROM mcr.microsoft.com/dotnet/sdk:6.0.402-alpine3.16 AS build
WORKDIR /app


COPY src/Altinn.AccessManagement/*.csproj ./src/Altinn.AccessManagement/
COPY src/Altinn.AccessManagement.Core/*.csproj ./src/Altinn.AccessManagement.Core/
COPY src/Altinn.AccessManagement.Integration/*.csproj ./src/Altinn.AccessManagement.Core/
COPY src/Altinn.AccessManagement.Persistance/*.csproj ./src/Altinn.AccessManagement.Persistance/
RUN dotnet restore ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj


# Copy everything else and build
COPY src/Altinn.AccessManagement/ ./src/Altinn.AccessManagement/
COPY src/Altinn.AccessManagement.Core/ ./src/Altinn.AccessManagement.Core/
COPY src/Altinn.AccessManagement.Integration/ ./src/Altinn.AccessManagement.Core/
COPY src/Altinn.AccessManagement.Persistance/ ./src/Altinn.AccessManagement.Persistance/
RUN dotnet publish -c Release -o out ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-alpine3.16 AS final
EXPOSE 5100
WORKDIR /app
COPY --from=build /app/out .

COPY src/Altinn.AccessManagement/Migration ./Migration

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry
ENTRYPOINT ["dotnet", "Altinn.Altinn.AccessManagement.dll"]
