namespace LoanApp.Domain.Rules;

public sealed record Decision(bool IsApproved, string? RuleCode, string? Reason)
{
    public static Decision Approved() => new(true, null, null);

    public static Decision Denied(string ruleCode, string reason) =>
        new(false, ruleCode, reason);
}
