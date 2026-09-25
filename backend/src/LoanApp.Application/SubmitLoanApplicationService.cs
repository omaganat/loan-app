using LoanApp.Application.Abstractions;
using LoanApp.Application.Contracts;
using LoanApp.Domain;
using LoanApp.Domain.Entities;
using LoanApp.Domain.Rules;

namespace LoanApp.Application;

public sealed class SubmitLoanApplicationService
{
    private static readonly HashSet<string> UsStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA","KS",
        "KY","LA","ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY",
        "NC","ND","OH","OK","OR","PA","RI","SC","SD","TN","TX","UT","VT","VA","WA","WV",
        "WI","WY","DC"
    };

    private readonly RuleEngine _ruleEngine;
    private readonly ICustomerStore _customers;
    private readonly IApplicationStore _applications;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitLoanApplicationService(
        RuleEngine ruleEngine,
        ICustomerStore customers,
        IApplicationStore applications,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _ruleEngine = ruleEngine;
        _customers = customers;
        _applications = applications;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitLoanApplicationResult> SubmitAsync(
        SubmitLoanApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return SubmitLoanApplicationResult.Invalid(errors.ToArray());
        }

        var normalizedSsn = Ssn.Normalize(request.Ssn);
        var state = request.State.Trim().ToUpperInvariant();
        var input = new LoanApplicationInput(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Address.Trim(),
            state,
            request.CompanyName.Trim(),
            request.RequestedAmount,
            normalizedSsn);

        var decision = _ruleEngine.Decide(input);
        if (!decision.IsApproved)
        {
            return SubmitLoanApplicationResult.Denied(decision.RuleCode!, decision.Reason!);
        }

        Guid customerId = Guid.Empty;
        Guid applicationId = Guid.Empty;
        var returning = false;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var existing = await _customers.FindBySsnAsync(normalizedSsn, ct);
            returning = existing is not null;
            Customer customer;

            if (existing is null)
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Address = input.Address,
                    State = input.State,
                    CompanyName = input.CompanyName,
                    Ssn = normalizedSsn,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _customers.Add(customer);
            }
            else
            {
                existing.UpdateFrom(
                    input.FirstName,
                    input.LastName,
                    input.Address,
                    input.State,
                    input.CompanyName);
                customer = existing;
            }

            var existingApplication = existing is null
                ? null
                : await _applications.FindByCustomerIdAsync(customer.Id, ct);

            LoanApplication application;
            if (existingApplication is null)
            {
                application = new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    RequestedAmount = input.RequestedAmount,
                    Status = "Approved",
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _applications.Add(application);
            }
            else
            {
                existingApplication.UpdateAmount(input.RequestedAmount);
                application = existingApplication;
            }

            _outbox.EnqueueCustomerUpsert(new ExternalCustomerPayload
            {
                CustomerId = customer.Id,
                ApplicationId = application.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Address = customer.Address,
                State = customer.State,
                CompanyName = customer.CompanyName,
                Ssn = customer.Ssn,
                RequestedAmount = application.RequestedAmount,
                IsUpdate = returning
            });

            customerId = customer.Id;
            applicationId = application.Id;
        }, cancellationToken);

        return SubmitLoanApplicationResult.Ok(customerId, applicationId, returning);
    }

    private static List<string> Validate(SubmitLoanApplicationRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName)) errors.Add("First name is required.");
        if (string.IsNullOrWhiteSpace(request.LastName)) errors.Add("Last name is required.");
        if (string.IsNullOrWhiteSpace(request.Address)) errors.Add("Address is required.");
        if (string.IsNullOrWhiteSpace(request.CompanyName)) errors.Add("Company name is required.");
        if (string.IsNullOrWhiteSpace(request.State))
        {
            errors.Add("State is required.");
        }
        else if (!UsStates.Contains(request.State.Trim()))
        {
            errors.Add("State must be a valid US state code.");
        }

        if (request.RequestedAmount <= 0)
        {
            errors.Add("Requested amount must be greater than zero.");
        }

        var ssn = Ssn.Normalize(request.Ssn);
        if (!Ssn.IsValid(ssn))
        {
            errors.Add("SSN must contain exactly 9 digits.");
        }

        return errors;
    }
}
