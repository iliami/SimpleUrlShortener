FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["*.props", "./"]
COPY ["src/SimpleUrlShortener.UrlLifetimeManager.DbMigrator/SimpleUrlShortener.UrlLifetimeManager.DbMigrator.csproj", "src/SimpleUrlShortener.UrlLifetimeManager.DbMigrator/"]
RUN dotnet restore "src/SimpleUrlShortener.UrlLifetimeManager.DbMigrator/SimpleUrlShortener.UrlLifetimeManager.DbMigrator.csproj"
COPY . .
WORKDIR "/src/src/SimpleUrlShortener.UrlLifetimeManager.DbMigrator"
RUN dotnet build "./SimpleUrlShortener.UrlLifetimeManager.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleUrlShortener.UrlLifetimeManager.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleUrlShortener.UrlLifetimeManager.DbMigrator.dll"]
