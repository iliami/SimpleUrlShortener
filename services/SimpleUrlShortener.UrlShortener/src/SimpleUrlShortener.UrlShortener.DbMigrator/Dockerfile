FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["*.props", "./"]
COPY ["src/SimpleUrlShortener.UrlShortener.DbMigrator/SimpleUrlShortener.UrlShortener.DbMigrator.csproj", "src/SimpleUrlShortener.UrlShortener.DbMigrator/"]
RUN dotnet restore "src/SimpleUrlShortener.UrlShortener.DbMigrator/SimpleUrlShortener.UrlShortener.DbMigrator.csproj"
COPY . .
WORKDIR "/src/src/SimpleUrlShortener.UrlShortener.DbMigrator"
RUN dotnet build "./SimpleUrlShortener.UrlShortener.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleUrlShortener.UrlShortener.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleUrlShortener.UrlShortener.DbMigrator.dll"]
