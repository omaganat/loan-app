using LoanApp.Application;
using LoanApp.Application.Contracts;
using LoanApp.Domain.Blacklist;
using LoanApp.Domain.Rules;
using LoanApp.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Tests;

public class SubmitLoanApplicationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly SubmitLoanApplicationService _service;

    public SubmitLoanApplicationServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        var engine = new RuleEngine(new IDenialRule[]
        {
            new NyStateDenialRule(),
            new BlacklistedSsnDenialRule(new StaticSsnBlacklist())
        });

        _service = new SubmitLoanApplicationService(
            engine,
            new CustomerStore(_db),
            new ApplicationStore(_db),
            new EfOutbox(_db),
            new EfUnitOfWork(_db));
    }

    [Fact]
    public async Task Returning_customer_updates_existing_records_instead_of_creating_new_ones()
    {
        var first = await _service.SubmitAsync(Request("123456789", "Ada", 1000, "CA"));
        var second = await _service.SubmitAsync(Request("123456789", "Ada Marie", 2500, "TX"));

        Assert.True(first.Approved);
        Assert.False(first.ReturningCustomer);
        Assert.True(second.Approved);
        Assert.True(second.ReturningCustomer);
        Assert.Equal(first.CustomerId, second.CustomerId);
        Assert.Equal(first.ApplicationId, second.ApplicationId);

        Assert.Equal(1, await _db.Customers.CountAsync());
        Assert.Equal(1, await _db.Applications.CountAsync());

        var customer = await _db.Customers.SingleAsync();
        var application = await _db.Applications.SingleAsync();
        Assert.Equal("Ada Marie", customer.FirstName);
        Assert.Equal("TX", customer.State);
        Assert.Equal(2500, application.RequestedAmount);
        Assert.Equal(2, await _db.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task Denied_application_does_not_persist_or_publish()
    {
        var result = await _service.SubmitAsync(Request("123456789", "Ada", 1000, "NY"));

        Assert.False(result.Approved);
        Assert.Equal("NY_STATE", result.DenialCode);
        Assert.Equal(0, await _db.Customers.CountAsync());
        Assert.Equal(0, await _db.Applications.CountAsync());
        Assert.Equal(0, await _db.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task Transaction_rolls_back_when_save_fails()
    {
        var first = await _service.SubmitAsync(Request("123456789", "Ada", 1000, "CA"));
        Assert.True(first.Approved);

        _db.ChangeTracker.Clear();
        await _db.Database.ExecuteSqlRawAsync("DROP TABLE OutboxMessages");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.SubmitAsync(Request("555555555", "Grace", 2000, "CA")));

        _db.ChangeTracker.Clear();
        Assert.Equal(1, await _db.Customers.CountAsync());
        Assert.Equal(1, await _db.Applications.CountAsync());
        Assert.Equal("Ada", (await _db.Customers.SingleAsync()).FirstName);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static SubmitLoanApplicationRequest Request(string ssn, string firstName, decimal amount, string state) => new()
    {
        FirstName = firstName,
        LastName = "Lovelace",
        Address = "1 Computing Lane",
        State = state,
        CompanyName = "Analytical Engines",
        RequestedAmount = amount,
        Ssn = ssn
    };
}
