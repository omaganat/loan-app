using LoanApp.Domain.Entities;

namespace LoanApp.Application.Abstractions;

public interface IApplicationStore
{
    Task<LoanApplication?> FindByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    void Add(LoanApplication application);
}
