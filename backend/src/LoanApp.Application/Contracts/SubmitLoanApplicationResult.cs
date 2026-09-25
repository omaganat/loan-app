namespace LoanApp.Application.Contracts;

public sealed class SubmitLoanApplicationResult
{
    public bool Approved { get; init; }
    public string? DenialCode { get; init; }
    public string? DenialReason { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ApplicationId { get; init; }
    public bool ReturningCustomer { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static SubmitLoanApplicationResult Denied(string code, string reason) => new()
    {
        Approved = false,
        DenialCode = code,
        DenialReason = reason
    };

    public static SubmitLoanApplicationResult Invalid(params string[] errors) => new()
    {
        Approved = false,
        Errors = errors
    };

    public static SubmitLoanApplicationResult Ok(Guid customerId, Guid applicationId, bool returning) => new()
    {
        Approved = true,
        CustomerId = customerId,
        ApplicationId = applicationId,
        ReturningCustomer = returning
    };
}
