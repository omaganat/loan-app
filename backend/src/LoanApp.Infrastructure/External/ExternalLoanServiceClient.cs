using System.Net.Http.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Contracts;

namespace LoanApp.Infrastructure.External;

public sealed class ExternalLoanServiceClient : IExternalLoanService
{
    private readonly HttpClient _http;

    public ExternalLoanServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task CreateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsJsonAsync("/customers", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken)
    {
        using var response = await _http.PutAsJsonAsync($"/customers/{payload.Ssn}", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
