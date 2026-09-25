using LoanApp.Domain.Entities;

namespace LoanApp.Application.Abstractions;

public interface ICustomerStore
{
    Task<Customer?> FindBySsnAsync(string normalizedSsn, CancellationToken cancellationToken);
    void Add(Customer customer);
}
