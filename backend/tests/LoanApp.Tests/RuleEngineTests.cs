using LoanApp.Domain.Blacklist;
using LoanApp.Domain.Rules;

namespace LoanApp.Tests;

public class RuleEngineTests
{
    private static RuleEngine CreateEngine(params string[] blockedSsns)
    {
        var blacklist = new FakeBlacklist(blockedSsns);
        return new RuleEngine(new IDenialRule[]
        {
            new NyStateDenialRule(),
            new BlacklistedSsnDenialRule(blacklist)
        });
    }

    [Fact]
    public void Denies_when_state_is_NY()
    {
        var engine = CreateEngine();
        var decision = engine.Decide(Input(state: "NY", ssn: "123456789"));

        Assert.False(decision.IsApproved);
        Assert.Equal("NY_STATE", decision.RuleCode);
    }

    [Fact]
    public void Denies_when_ssn_is_blacklisted()
    {
        var engine = CreateEngine("111111111");
        var decision = engine.Decide(Input(state: "CA", ssn: "111111111"));

        Assert.False(decision.IsApproved);
        Assert.Equal("BLACKLISTED_SSN", decision.RuleCode);
    }

    [Fact]
    public void Approves_when_no_rule_matches()
    {
        var engine = CreateEngine("111111111");
        var decision = engine.Decide(Input(state: "CA", ssn: "123456789"));

        Assert.True(decision.IsApproved);
        Assert.Null(decision.RuleCode);
    }

    [Fact]
    public void Adding_a_new_rule_does_not_change_existing_rules()
    {
        var extra = new AlwaysDenyCompanyRule("ACME");
        var engine = new RuleEngine(new IDenialRule[]
        {
            new NyStateDenialRule(),
            extra
        });

        var ny = engine.Decide(Input(state: "NY", company: "Other"));
        var company = engine.Decide(Input(state: "CA", company: "ACME"));
        var ok = engine.Decide(Input(state: "CA", company: "Other"));

        Assert.Equal("NY_STATE", ny.RuleCode);
        Assert.Equal("BLOCKED_COMPANY", company.RuleCode);
        Assert.True(ok.IsApproved);
    }

    private static LoanApplicationInput Input(
        string state,
        string ssn = "123456789",
        string company = "Acme") =>
        new("Ada", "Lovelace", "1 Main St", state, company, 5000, ssn);

    private sealed class FakeBlacklist : ISsnBlacklist
    {
        private readonly HashSet<string> _blocked;
        public FakeBlacklist(IEnumerable<string> blocked) => _blocked = new HashSet<string>(blocked);
        public bool Contains(string normalizedSsn) => _blocked.Contains(normalizedSsn);
    }

    private sealed class AlwaysDenyCompanyRule : IDenialRule
    {
        private readonly string _company;
        public AlwaysDenyCompanyRule(string company) => _company = company;
        public string Code => "BLOCKED_COMPANY";
        public Decision? Evaluate(LoanApplicationInput input) =>
            string.Equals(input.CompanyName, _company, StringComparison.OrdinalIgnoreCase)
                ? Decision.Denied(Code, "Company is not eligible.")
                : null;
    }
}
