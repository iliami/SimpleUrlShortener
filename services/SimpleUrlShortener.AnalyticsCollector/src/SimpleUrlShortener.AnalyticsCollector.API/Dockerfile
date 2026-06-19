FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SimpleUrlShortener.AnalyticsCollector.API/SimpleUrlShortener.AnalyticsCollector.API.csproj", "src/SimpleUrlShortener.AnalyticsCollector.API/"]
COPY ["src/SimpleUrlShortener.AnalyticsCollector.Domain/SimpleUrlShortener.AnalyticsCollector.Domain.csproj", "src/SimpleUrlShortener.AnalyticsCollector.Domain/"]
COPY ["src/SimpleUrlShortener.AnalyticsCollector.Infrastructure/SimpleUrlShortener.AnalyticsCollector.Infrastructure.csproj", "src/SimpleUrlShortener.AnalyticsCollector.Infrastructure/"]
RUN dotnet restore "src/SimpleUrlShortener.AnalyticsCollector.API/SimpleUrlShortener.AnalyticsCollector.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleUrlShortener.AnalyticsCollector.API"
RUN dotnet build "./SimpleUrlShortener.AnalyticsCollector.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleUrlShortener.AnalyticsCollector.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleUrlShortener.AnalyticsCollector.API.dll"]
