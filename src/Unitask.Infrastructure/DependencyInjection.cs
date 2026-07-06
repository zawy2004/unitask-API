using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Settings;
using Unitask.Infrastructure.Persistence;
using Unitask.Infrastructure.Repositories;
using Unitask.Infrastructure.Services;

namespace Unitask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<UnitaskDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("UnitaskDb"), sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)));

        services.AddMemoryCache();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<IGroqChatClient, GroqChatClient>();
        services.AddScoped<IInsightsService, InsightsService>();
        services.AddScoped<IMilestoneService, MilestoneService>();
        services.AddScoped<IDisputeService, DisputeService>();

        // Email (SMTP / Gmail App Password)
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
