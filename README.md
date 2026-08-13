# SimpleUrlShortener

[EN](./README.EN.md)

Yet Another Url Shortener — платформа для сокращения URL-адресов, реализованная в виде трех независимых микросервисов на базе ASP.NET Core / .NET 10.

Система позволяет создавать короткие URL-адреса и переходить по ним, при этом в асинхронном режиме собирая аналитику по каждому клику (включая геолокационные данные на основе IP-адреса) и применяя политику времени жизни, которая автоматически аннулирует и удаляет неактивные маппинги URL-адресов.

---

## Архитектура

Каждый сервис размещается в собственном решении в директории `services/` и следует единой многоуровневой архитектуре **Clean Architecture**:

```
services/<ServiceName>/
├── <ServiceName>.sln
├── Directory.Build.props
├── Directory.Packages.props
└── src/
    ├── <ServiceName>.API
    ├── <ServiceName>.Domain
    ├── <ServiceName>.Infrastructure
    └── <ServiceName>.DbMigrator      (консольное приложение для миграций)
```

| Сервис | Обязанности | Порт Docker |
|---------|----------------|-------------|
| `UrlShortener` | Создание / разрешение / удаление коротких URL-адресов; публикация событий `url.*` | `25000` (a), `25001` (b) через nginx `:80` |
| `AnalyticsCollector` | Потребление событий перенаправления, сбор аналитики и геоданных | `25002` |
| `UrlLifetimeManager` | Потребление событий, аннулирование неактивных URL-адресов, вызов UrlShortener для удаления | `25003` |

- **CQRS** через `Mediator` (source-generated MediatR).
- **Событийно-ориентированная архитектура**: UrlShortener публикует события `url.*` в RabbitMQ; AnalyticsCollector и UrlLifetimeManager их потребляют.
- **Горизонтальное масштабирование**: два экземпляра `UrlShortener` (`a`, `b`) за балансировщиком нагрузки `nginx`; каждый экземпляр идентифицируется с помощью `UrlShortenerSettings__InstancePrefix`, который также используется в качестве префикса для генерируемых коротких кодов.
- **Аутентификация по API-ключу**: заголовок `X-API-KEY` для эндпоинтов DELETE (но это пока не очень безопасно).
- **Короткие коды**: 6 случайных символов из 62-символьного алфавита с префиксом экземпляра.

---

## Как начать работу

```shell
git clone https://github.com/iliami/SimpleUrlShortener.git
cd SimpleUrlShortener
cd deploy
chmod -R 755 configs/observability
docker compose up -d
```

`DbMigrator` для каждого сервиса запускается как init-контейнер, поэтому выполнение миграций вручную не требуется.

---

## Порты сервисов и доступ

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

Сети сегментированы: `backend`, `database`, `messaging`, `monitoring`. Публичные порты сервисов (`25000`–`25003`, `80`) проброшены наружу; порты управления и UI (`15672`, `24999`, `25300`) привязаны к `127.0.0.1`.

---

## Технологии

C#, Mediator, Serilog, ASP.NET Core, Swagger, EntityFrameworkCore, Npgsql, Polly, RabbitMQ.Client, OpenTelemetry

PostgreSQL, PgBouncer, RabbitMQ, nginx, pgAdmin, Grafana, Loki, Tempo, Prometheus, OpenTelemetry Collector