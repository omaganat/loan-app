using LoanApp.Application.Abstractions;
using LoanApp.Domain.Blacklist;
using LoanApp.Infrastructure.External;
using LoanApp.Infrastructure.Messaging;
using LoanApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Data Source=loanapp.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ICustomerStore, CustomerStore>();
        services.AddScoped<IApplicationStore, ApplicationStore>();
        services.AddScoped<IOutbox, EfOutbox>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<ISsnBlacklist, StaticSsnBlacklist>();

        var externalBaseUrl = configuration["ExternalService:BaseUrl"] ?? "http://localhost:4000";
        services.AddHttpClient<IExternalLoanService, ExternalLoanServiceClient>(client =>
        {
            client.BaseAddress = new Uri(externalBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHostedService<OutboxProcessor>();
        return services;
    }
}
