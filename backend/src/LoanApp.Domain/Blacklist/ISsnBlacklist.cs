namespace LoanApp.Domain.Blacklist;

public interface ISsnBlacklist
{
    bool Contains(string normalizedSsn);
}
