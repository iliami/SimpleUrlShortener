# SimpleUrlShortener

Yet Another Url Shortener

---

## How to start

```
docker run --name SimpleUrlShortener.RabbitMQ -p 5672:5672 rabbitmq:4
docker run --name SimpleUrlShortener.Postgres -e POSTGRES_USER=root -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:17
git clone https://github.com/iliami/SimpleUrlShortener.git
cd SimpleUrlShortener
dotnet run --project SimpleUrlShortener.Presentation -lp http
```
