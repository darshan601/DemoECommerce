using Authentication.Infrastructure.Data;
using Authentication.Infrastructure.Repositories;
using AuthenticationApi.Application.Interfaces;
using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
    {
        
        // Add database connectivity
        // JWT add authentication scheme
        SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config,
            config["MySerilog:FileName"]!);
        
        // create dependency injection
        services.AddScoped<IUser, UserRepository>();
        
        return services;
    }

    public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
    {
        // register middleware such as:
        // global exception: handle external errors
        // listen only to api gateway :block all outsiders call

        SharedServiceContainer.UseSharedPolicies(app);

        return app!;

    }
}