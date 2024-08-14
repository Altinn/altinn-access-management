FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Copy everything and build
COPY . .
RUN dotnet restore

# Copy everything else and build
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
