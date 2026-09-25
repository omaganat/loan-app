using LoanApp.Application.Contracts;

namespace LoanApp.Application.Abstractions;

public interface IExternalLoanService
{
    Task CreateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken);
    Task UpdateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken);
}
