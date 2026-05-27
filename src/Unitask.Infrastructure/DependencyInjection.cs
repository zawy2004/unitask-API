using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unitask.Application.Common.Interfaces;
using Unitask.Infrastructure.Persistence;
using Unitask.Infrastructure.Repositories;
using Unitask.Infrastructure.Services;

namespace Unitask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UnitaskDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("UnitaskDb")));

        services.AddMemoryCache();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<IGroqChatClient, GroqChatClient>();
        services.AddScoped<IInsightsService, InsightsService>();

        return services;
    }
}
