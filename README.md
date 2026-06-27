# SimpleUrlShortener

Yet Another Url Shortener

---

## How to start

```shell
git clone https://github.com/iliami/SimpleUrlShortener.git
cd SimpleUrlShortener
cd deploy
chmod -R 755 configs/observability
docker compose up -d
```

| Service Name                     | Access Zone | Port  |
|:---------------------------------|:------------|:------|
| url-shortener                    | Public      | 25000 |
| analytics-collector              | Public      | 25001 |
| url-lifetime-manager             | Public      | 25002 |
| rabbitmq                         | Local       | 15672 |
| pgadmin                          | Local       | 24999 |
| grafana                          | Local       | 25300 |
| postgresql                       | Internal    | 5432  |
| otel-collector                   | Internal    | 4317  |
| prometheus                       | Internal    | 9090  |
| loki                             | Internal    | 3100  |
| tempo                            | Internal    | 3200  |
| db-migrator_url-shortener        | Internal    | N/A   |
| db-migrator_analytics-collector  | Internal    | N/A   |
| db-migrator_url-lifetime-manager | Internal    | N/A   |

---

## Technologies

C#, Mediator, Serilog, ASP.NET Core, Swagger, EntityFrameworkCore, Npgsql, Polly, RabbitMQ.Client

PostreSQL, pgAdmin, RabbitMQ, Grafana, Loki, Tempo, Prometheus, OpenTelemetry Collector
