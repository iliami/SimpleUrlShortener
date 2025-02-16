# SimpleUrlShortener

Yet Another Url Shortener

---

## How to start

```
docker run --name SimpleUrlShortener.RabbitMQ -p 5672:5672 rabbitmq:4
docker run --name SimpleUrlShortener.Postgres -e POSTGRES_USER=root -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:17
git clone https://github.com/iliami/SimpleUrlShortener.git
cd SimpleUrlShortener
```

Url Shortener:
```
dotnet run --project SimpleUrlShortener.UrlShortener.Presentation -lp http
```

Analytics Data:
```
dotnet run --project SimpleUrlShortener.Analytics -lp http
```

---

## Technologies

Serilog, MediatR, FluentValidation, EF Core, PostgreSQL, MassTransit, RabbitMQ, Docker