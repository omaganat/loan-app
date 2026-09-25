using LoanApp.Application.Abstractions;
using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence;

public sealed class CustomerStore : ICustomerStore
{
    private readonly AppDbContext _db;

    public CustomerStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<Customer?> FindBySsnAsync(string normalizedSsn, CancellationToken cancellationToken) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Ssn == normalizedSsn, cancellationToken);

    public void Add(Customer customer) => _db.Customers.Add(customer);
}
