FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SimpleUrlShortener.UrlLifetimeManager.API/SimpleUrlShortener.UrlLifetimeManager.API.csproj", "src/SimpleUrlShortener.UrlLifetimeManager.API/"]
COPY ["src/SimpleUrlShortener.UrlLifetimeManager.Domain/SimpleUrlShortener.UrlLifetimeManager.Domain.csproj", "src/SimpleUrlShortener.UrlLifetimeManager.Domain/"]
COPY ["src/SimpleUrlShortener.UrlLifetimeManager.Infrastructure/SimpleUrlShortener.UrlLifetimeManager.Infrastructure.csproj", "src/SimpleUrlShortener.UrlLifetimeManager.Infrastructure/"]
RUN dotnet restore "src/SimpleUrlShortener.UrlLifetimeManager.API/SimpleUrlShortener.UrlLifetimeManager.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleUrlShortener.UrlLifetimeManager.API"
RUN dotnet build "./SimpleUrlShortener.UrlLifetimeManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleUrlShortener.UrlLifetimeManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleUrlShortener.UrlLifetimeManager.API.dll"]
