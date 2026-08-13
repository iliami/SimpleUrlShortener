# SimpleUrlShortener

Yet Another Url Shortener — a URL-shortening platform built as three independent microservices on ASP.NET Core / .NET 10.

The system lets you create short URLs and redirect them, while asynchronously collecting per-click analytics (including geo/coordinate data based on IP) and enforcing a lifetime policy that automatically expires and deletes inactive URL mappings.

---

## Architecture

Each service lives in its own solution under `services/` and follows the same **Clean Architecture** layering:

```
services/<ServiceName>/
├── <ServiceName>.sln
├── Directory.Build.props
├── Directory.Packages.props
└── src/
    ├── <ServiceName>.API
    ├── <ServiceName>.Domain
    ├── <ServiceName>.Infrastructure
    └── <ServiceName>.DbMigrator      (console app for migrations)
```

| Service | Responsibility | Docker port |
|---------|----------------|-------------|
| `UrlShortener` | Create / resolve / delete short URLs; publishes `url.*` events | `25000` (a), `25001` (b) via nginx `:80` |
| `AnalyticsCollector` | Consume redirect events, collect analytics + geo data | `25002` |
| `UrlLifetimeManager` | Consume events, expire inactive URLs, call UrlShortener to delete | `25003` |

- **CQRS** via `Mediator` (source-generated MediatR).
- **Event-driven**: UrlShortener publishes `url.*` events to RabbitMQ; AnalyticsCollector and UrlLifetimeManager consume them.
- **Horizontal scaling**: two `UrlShortener` instances (`a`, `b`) behind an `nginx` load balancer; each instance is distinguished by `UrlShortenerSettings__InstancePrefix`, which is also used as a prefix on generated short codes.
- **API key auth**: `X-API-KEY` header for DELETE endpoints (but it's not that much secure).
- **Short codes**: 6-char random over a 62-char alphabet, prefixed by the instance prefix.

---

## How to start

```shell
git clone https://github.com/iliami/SimpleUrlShortener.git
cd SimpleUrlShortener
cd deploy
chmod -R 755 configs/observability
docker compose up -d
```

`DbMigrator` for each service runs as an init container — so no manual migration step required.

---

## Service Ports & Access

| Service Name                     | Access Zone | Port  |
|:---------------------------------|:------------|:------|
| nginx (public entry point)       | Public      | 80    |
| url-shortener-a                  | Public      | 25000 |
| url-shortener-b                  | Public      | 25001 |
| analytics-collector              | Public      | 25002 |
| url-lifetime-manager             | Public      | 25003 |
| rabbitmq (management UI)         | Local       | 15672 |
| pgadmin                          | Local       | 24999 |
| grafana                          | Local       | 25300 |
| postgresql                       | Internal    | 5432  |
| pgbouncer                        | Internal    | 6432  |
| otel-collector                   | Internal    | 4317  |
| prometheus                       | Internal    | 9090  |
| loki                             | Internal    | 3100  |
| tempo                            | Internal    | 3200  |
| db-migrator_url-shortener        | Internal    | N/A   |
| db-migrator_analytics-collector  | Internal    | N/A   |
| db-migrator_url-lifetime-manager | Internal    | N/A   |

Networks are segmented: `backend`, `database`, `messaging`, `monitoring`. Public-facing service ports (`25000`–`25003`, `80`) are published; management/UI ports (`15672`, `24999`, `25300`) are bound to `127.0.0.1`.

---

## Technologies

C#, Mediator, Serilog, ASP.NET Core, Swagger, EntityFrameworkCore, Npgsql, Polly, RabbitMQ.Client, OpenTelemetry

PostgreSQL, PgBouncer, RabbitMQ, nginx, pgAdmin, Grafana, Loki, Tempo, Prometheus, OpenTelemetry Collector
