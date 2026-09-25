using System.Text.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Contracts;
using LoanApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanApp.Infrastructure.Messaging;

public sealed class OutboxProcessor : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Outbox processor failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var external = scope.ServiceProvider.GetRequiredService<IExternalLoanService>();

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                if (message.EventType == EfOutbox.CustomerUpserted)
                {
                    var payload = JsonSerializer.Deserialize<ExternalCustomerPayload>(message.Payload, JsonOptions)
                                  ?? throw new InvalidOperationException("Invalid outbox payload.");

                    if (payload.IsUpdate)
                    {
                        await external.UpdateAsync(payload, cancellationToken);
                    }
                    else
                    {
                        await external.CreateAsync(payload, cancellationToken);
                    }
                }

                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount += 1;
                message.LastError = ex.Message;
                _logger.LogWarning(ex, "Failed to dispatch outbox message {Id}", message.Id);
            }
        }

        if (pending.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
