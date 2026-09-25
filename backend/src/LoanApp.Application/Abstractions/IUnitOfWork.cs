namespace LoanApp.Application.Abstractions;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}
