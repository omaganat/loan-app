using LoanApp.Application.Abstractions;
using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence;

public sealed class ApplicationStore : IApplicationStore
{
    private readonly AppDbContext _db;

    public ApplicationStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<LoanApplication?> FindByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        _db.Applications.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken);

    public void Add(LoanApplication application) => _db.Applications.Add(application);
}
