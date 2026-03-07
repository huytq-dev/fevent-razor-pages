using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddInfrastructureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
            services.AddScoped<TimestampInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            options.AddInterceptors(sp.GetRequiredService<TimestampInterceptor>());
        });
        return services;
    }
}
