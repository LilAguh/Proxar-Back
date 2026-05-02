using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using Services.Implementations;
using Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace Config;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBoxMovementRepository, BoxMovementRepository>();
        services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
{
    services.AddScoped<ICompanyService, CompanyService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IClientService, ClientService>();
    services.AddScoped<ITicketService, TicketService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IBoxMovementService, BoxMovementService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<ICashRegisterService, CashRegisterService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<ISubscriptionService, SubscriptionService>();

    return services;
}

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        return services;
    }
}