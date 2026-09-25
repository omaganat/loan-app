namespace LoanApp.Domain.Entities;

public class LoanApplication
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal RequestedAmount { get; set; }
    public string Status { get; set; } = "Approved";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Customer? Customer { get; set; }

    public void UpdateAmount(decimal requestedAmount)
    {
        RequestedAmount = requestedAmount;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
