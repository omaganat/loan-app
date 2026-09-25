using LoanApp.Domain.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<NyStateDenialRule>();
        services.AddSingleton<BlacklistedSsnDenialRule>();
        services.AddSingleton<IDenialRule>(sp => sp.GetRequiredService<NyStateDenialRule>());
        services.AddSingleton<IDenialRule>(sp => sp.GetRequiredService<BlacklistedSsnDenialRule>());
        services.AddSingleton<RuleEngine>();
        services.AddScoped<SubmitLoanApplicationService>();
        return services;
    }
}
