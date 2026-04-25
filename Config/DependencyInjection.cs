using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using Services.Implementations;
using Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Config;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBoxMovementRepository, BoxMovementRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IBoxMovementService, BoxMovementService>();

        return services;
    }

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        return services;
    }
}