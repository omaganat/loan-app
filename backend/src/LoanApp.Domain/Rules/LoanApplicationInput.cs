namespace LoanApp.Domain.Rules;

public sealed record LoanApplicationInput(
    string FirstName,
    string LastName,
    string Address,
    string State,
    string CompanyName,
    decimal RequestedAmount,
    string Ssn);
