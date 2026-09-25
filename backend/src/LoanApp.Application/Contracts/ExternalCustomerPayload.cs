namespace LoanApp.Application.Contracts;

public sealed class ExternalCustomerPayload
{
    public Guid CustomerId { get; set; }
    public Guid ApplicationId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public bool IsUpdate { get; set; }
}
