using LoanApp.Domain;
using LoanApp.Domain.Blacklist;

namespace LoanApp.Infrastructure.Persistence;

public sealed class StaticSsnBlacklist : ISsnBlacklist
{
    // Seeded ineligible SSNs used by the take-home test data.
    private static readonly HashSet<string> Blocked = new()
    {
        "111111111",
        "000000000",
        "999999999"
    };

    public bool Contains(string normalizedSsn) => Blocked.Contains(Ssn.Normalize(normalizedSsn));
}
