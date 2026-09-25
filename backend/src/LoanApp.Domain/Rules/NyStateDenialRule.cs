namespace LoanApp.Domain.Rules;

public sealed class NyStateDenialRule : IDenialRule
{
    public string Code => "NY_STATE";

    public Decision? Evaluate(LoanApplicationInput input)
    {
        if (string.Equals(input.State.Trim(), "NY", StringComparison.OrdinalIgnoreCase))
        {
            return Decision.Denied(Code, "Applications from New York are not accepted.");
        }

        return null;
    }
}
