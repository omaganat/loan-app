using LoanApp.Application.Contracts;

namespace LoanApp.Application.Abstractions;

public interface IOutbox
{
    void EnqueueCustomerUpsert(ExternalCustomerPayload payload);
}
