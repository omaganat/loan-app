using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LoanApp.Tests;

public class ApplicationsEndpointTests : IClassFixture<LoanApiFactory>
{
    private readonly HttpClient _client;

    public ApplicationsEndpointTests(LoanApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_returns_approved_payload()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new
        {
            firstName = "Grace",
            lastName = "Hopper",
            address = "88 Navy St",
            state = "VA",
            companyName = "COBOL Corp",
            requestedAmount = 8000,
            ssn = "222-33-4444"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("approved").GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("applicationId", out _));
    }

    [Fact]
    public async Task Post_returns_denial_for_ny()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new
        {
            firstName = "Grace",
            lastName = "Hopper",
            address = "1 Broadway",
            state = "NY",
            companyName = "COBOL Corp",
            requestedAmount = 8000,
            ssn = "333444555"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(doc.RootElement.GetProperty("approved").GetBoolean());
        Assert.Equal("NY_STATE", doc.RootElement.GetProperty("denialCode").GetString());
    }

    [Fact]
    public async Task Post_returns_bad_request_for_invalid_ssn()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new
        {
            firstName = "Grace",
            lastName = "Hopper",
            address = "88 Navy St",
            state = "VA",
            companyName = "COBOL Corp",
            requestedAmount = 8000,
            ssn = "12"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class LoanApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"loan-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("ExternalService:BaseUrl", "http://localhost:3999");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IExternalLoanService>();
            services.AddSingleton<IExternalLoanService, FakeExternalLoanService>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private sealed class FakeExternalLoanService : IExternalLoanService
    {
        public Task CreateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(ExternalCustomerPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
