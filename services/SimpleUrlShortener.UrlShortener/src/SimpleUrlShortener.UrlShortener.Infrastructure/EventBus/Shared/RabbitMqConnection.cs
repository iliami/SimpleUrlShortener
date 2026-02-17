using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared;

#pragma warning disable CA1848

public sealed class RabbitMqConnection(
    RabbitMqConnectionSettings settings,
    ILogger<RabbitMqConnection> logger)
    : IDisposable
{
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public async Task<IConnection> GetConnectionAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(RabbitMqConnection));

        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                _connection.Dispose();
                _connection = null;
            }

            return await CreateConnectionAsync();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            VirtualHost = settings.VirtualHost,
            UserName = settings.Username,
            Password = settings.Password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        for (var attempt = 1; attempt <= settings.ConnectionRetryCount; attempt++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();

                _connection.ConnectionShutdownAsync += (_, args) =>
                {
                    logger.LogWarning("RabbitMQ connection shutdown: {Reason}", args.ReplyText);
                    return Task.CompletedTask;
                };

                _connection.ConnectionRecoveryErrorAsync += (_, args) =>
                {
                    logger.LogError("RabbitMQ connection recovery error: {Error}", args.Exception.Message);
                    return Task.CompletedTask;
                };

                _connection.CallbackExceptionAsync += (_, args) =>
                {
                    logger.LogError("RabbitMQ callback exception: {Error}", args.Exception.Message);
                    return Task.CompletedTask;
                };

                logger.LogInformation("Successfully connected to RabbitMQ");
                return _connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{Total})",
                    attempt, settings.ConnectionRetryCount);

                if (attempt == settings.ConnectionRetryCount)
                {
                    throw;
                }

                await Task.Delay(settings.ConnectionRetryIntervalMs);
            }
        }

        throw new InvalidOperationException("Failed to establish RabbitMQ connection after all retries");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
        finally
        {
            _disposed = true;
            _connection = null;
            _connectionLock.Dispose();
        }
    }
}

#pragma warning restore CA1848