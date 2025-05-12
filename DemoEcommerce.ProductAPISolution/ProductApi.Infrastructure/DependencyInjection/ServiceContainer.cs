using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Application.Interfaces;
using ProductApi.Infrastructure.Data;
using ProductApi.Infrastructure.Repositories;

namespace ProductApi.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
    {
        // Add database connectivity
        // Add authentication scheme
        SharedServiceContainer.AddSharedServices<ProductDbContext>(services, config, config["MySerilog.FileName"]!);
        
        // create dependecyinjection
        services.AddScoped<IProduct, ProductRepository>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
    {
        // register middleware such as :
        // global exception: handles external errors
        // listen to only api gateway: blocks all outside calls
        SharedServiceContainer.UseSharedPolicies(app);

        return app;
    }
}