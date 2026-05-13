using Microsoft.Extensions.DependencyInjection;
using Unitask.Application.Mappings;

namespace Unitask.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }
}
