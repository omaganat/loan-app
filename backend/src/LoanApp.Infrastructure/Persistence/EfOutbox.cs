using System.Text.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Contracts;
using LoanApp.Domain.Entities;

namespace LoanApp.Infrastructure.Persistence;

public sealed class EfOutbox : IOutbox
{
    public const string CustomerUpserted = "CustomerUpserted";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppDbContext _db;

    public EfOutbox(AppDbContext db)
    {
        _db = db;
    }

    public void EnqueueCustomerUpsert(ExternalCustomerPayload payload)
    {
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = CustomerUpserted,
            Payload = JsonSerializer.Serialize(payload, JsonOptions),
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
