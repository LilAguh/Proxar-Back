using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
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
}