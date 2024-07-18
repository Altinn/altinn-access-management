FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

COPY src/Altinn.AccessManagement/*.csproj ./src/Altinn.AccessManagement/
COPY src/Altinn.AccessManagement.Core/*.csproj ./src/Altinn.AccessManagement.Core/
COPY src/Altinn.AccessManagement.Integration/*.csproj ./src/Altinn.AccessManagement.Integration/
COPY src/Altinn.AccessManagement.Persistence/*.csproj ./src/Altinn.AccessManagement.Persistence/
RUN dotnet restore ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj


# Copy everything else and build
COPY src ./src
RUN dotnet publish -c Release -o out ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
EXPOSE 5110
WORKDIR /app
COPY --from=build /app/out .

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry
ENTRYPOINT ["dotnet", "Altinn.AccessManagement.dll"]
