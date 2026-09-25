using LoanApp.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public EfUnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
