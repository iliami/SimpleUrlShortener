FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SimpleUrlShortener.UrlShortener.API/SimpleUrlShortener.UrlShortener.API.csproj", "src/SimpleUrlShortener.UrlShortener.API/"]
COPY ["src/SimpleUrlShortener.UrlShortener.Domain/SimpleUrlShortener.UrlShortener.Domain.csproj", "src/SimpleUrlShortener.UrlShortener.Domain/"]
COPY ["src/SimpleUrlShortener.UrlShortener.Infrastructure/SimpleUrlShortener.UrlShortener.Infrastructure.csproj", "src/SimpleUrlShortener.UrlShortener.Infrastructure/"]
RUN dotnet restore "src/SimpleUrlShortener.UrlShortener.API/SimpleUrlShortener.UrlShortener.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleUrlShortener.UrlShortener.API"
RUN dotnet build "./SimpleUrlShortener.UrlShortener.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleUrlShortener.UrlShortener.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleUrlShortener.UrlShortener.API.dll"]
