FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine@sha256:26a8e9c8c9854b0543a3ddaacb56ba96caaf47c4482844e5f7ad1f70338b97e3 AS build
WORKDIR /app

# Copy everything and build
COPY . .
RUN dotnet restore

# Copy everything else and build
RUN dotnet publish -c Release -o out ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine@sha256:be25345551555a8d61b91b603e8ee08a5a7d23779ca8c1e84028fdb551b950ad AS final
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
