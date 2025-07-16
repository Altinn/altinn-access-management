FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine@sha256:ad6ab1a677854e5b681bc69f4fdb4ad9913e3798c4f89927254b6a53215d5b7d AS build
WORKDIR /app

# Copy everything and build
COPY . .
RUN dotnet restore

# Copy everything else and build
RUN dotnet publish -c Release -o out ./src/Altinn.AccessManagement/Altinn.AccessManagement.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine@sha256:a6695199ce879ae4bcfc92f3dfd55b7db5e46e3bdc128a690c6594444f0e300a AS final
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
