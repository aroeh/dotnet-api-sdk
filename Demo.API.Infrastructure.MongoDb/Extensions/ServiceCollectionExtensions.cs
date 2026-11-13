using Demo.API.Infrastructure.MongoDb.Interfaces;
using Demo.API.Infrastructure.MongoDb.Repos;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.API.Infrastructure.MongoDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureRepos(this IServiceCollection services)
    {
        services.AddScoped<IRestuarantRepo, RestuarantRepo>();

        return services;
    }
}
