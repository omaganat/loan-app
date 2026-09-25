namespace LoanApp.Domain.Rules;

public sealed class RuleEngine
{
    private readonly IReadOnlyList<IDenialRule> _rules;

    public RuleEngine(IEnumerable<IDenialRule> rules)
    {
        _rules = rules.ToList();
    }

    public Decision Decide(LoanApplicationInput input)
    {
        foreach (var rule in _rules)
        {
            var denial = rule.Evaluate(input);
            if (denial is not null)
            {
                return denial;
            }
        }

        return Decision.Approved();
    }
}
