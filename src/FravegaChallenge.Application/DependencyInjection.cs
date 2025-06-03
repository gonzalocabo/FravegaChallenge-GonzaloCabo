using FravegaChallenge.Application.Interfaces;
using FravegaChallenge.Application.Orders;
using FravegaChallenge.Application.Services;
using Mapster;
using MapsterMapper;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrdersService, OrdersService>();
        services.AddScoped<IEventsService, EventsService>();

        var config = new TypeAdapterConfig();
        config.AddCustomMappings();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
