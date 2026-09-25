namespace LoanApp.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public LoanApplication? Application { get; set; }

    public void UpdateFrom(
        string firstName,
        string lastName,
        string address,
        string state,
        string companyName)
    {
        FirstName = firstName;
        LastName = lastName;
        Address = address;
        State = state;
        CompanyName = companyName;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
