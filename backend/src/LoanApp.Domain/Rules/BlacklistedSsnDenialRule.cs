using LoanApp.Domain.Blacklist;

namespace LoanApp.Domain.Rules;

public sealed class BlacklistedSsnDenialRule : IDenialRule
{
    private readonly ISsnBlacklist _blacklist;

    public BlacklistedSsnDenialRule(ISsnBlacklist blacklist)
    {
        _blacklist = blacklist;
    }

    public string Code => "BLACKLISTED_SSN";

    public Decision? Evaluate(LoanApplicationInput input)
    {
        if (_blacklist.Contains(input.Ssn))
        {
            return Decision.Denied(Code, "The provided SSN is not eligible.");
        }

        return null;
    }
}
