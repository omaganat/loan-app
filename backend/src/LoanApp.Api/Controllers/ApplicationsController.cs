using LoanApp.Application;
using LoanApp.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly SubmitLoanApplicationService _service;

    public ApplicationsController(SubmitLoanApplicationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitLoanApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.SubmitAsync(request, cancellationToken);

        if (result.Errors.Count > 0)
        {
            return BadRequest(new { errors = result.Errors });
        }

        if (!result.Approved)
        {
            return Ok(new
            {
                approved = false,
                denialCode = result.DenialCode,
                denialReason = result.DenialReason
            });
        }

        return Ok(new
        {
            approved = true,
            customerId = result.CustomerId,
            applicationId = result.ApplicationId,
            returningCustomer = result.ReturningCustomer
        });
    }
}
