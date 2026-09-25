namespace LoanApp.Domain.Rules;

public interface IDenialRule
{
    string Code { get; }
    Decision? Evaluate(LoanApplicationInput input);
}
