using FluentValidation.AspNetCore;

namespace Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var appAssembly = AssemblyReference.Assembly;

        services.AddServicesFromAssembly(appAssembly)
            .AddMapsterServices()
            .AddValidatorsFromAssembly(AssemblyReference.Assembly)
            .AddFluentValidationAutoValidation()        // ← server-side
            .AddFluentValidationClientsideAdapters();

        return services;
    }
}
