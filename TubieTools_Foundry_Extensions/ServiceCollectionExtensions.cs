namespace TubieTools_Foundry_Extensions.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;  
using TubieTools_Foundry_Extensions.Configuration;
using TubieTools_Foundry_Extensions.Repository;
using TubieTools_Foundry_Extensions.Services;

/// <summary>
/// Extension methods for registering Foundry services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Foundry extension services to the service collection.
    /// </summary>
    public static IServiceCollection AddFoundryExtension(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FoundryExtensionOptions>(options =>
            configuration.GetSection(FoundryExtensionOptions.SectionName).Bind(options));

        services.AddSingleton<IModelRepository, InMemoryModelRepository>();
        services.AddScoped<IFoundryModelService, FoundryModelService>();

        return services;
    }

    /// <summary>
    /// Adds the Foundry extension services with custom repository implementation.
    /// </summary>
    public static IServiceCollection AddFoundryExtension<TRepository>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TRepository : class, IModelRepository
    {
        services.Configure<FoundryExtensionOptions>(options =>
            configuration.GetSection(FoundryExtensionOptions.SectionName).Bind(options));

        services.AddSingleton<IModelRepository, TRepository>();
        services.AddScoped<IFoundryModelService, FoundryModelService>();

        return services;
    }
}