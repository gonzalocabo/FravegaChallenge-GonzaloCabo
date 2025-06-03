using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Infrastructure.Repositories;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMongoClient>(new MongoClient(configuration.GetSection("MongoDB")["ConnectionString"]));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(configuration.GetSection("MongoDB")["Database"]));

        services.AddScoped<ICountersRepository, CountersRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IEventsRepository, EventsRepository>();

        return services;
    }

    public static async Task InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var collections = await (await db.ListCollectionNamesAsync()).ToListAsync();
        await Task.WhenAll(SetupOrdersCollection(collections, db), SetupEventsCollection(collections, db));
    }

    private static async Task SetupOrdersCollection(List<string> collections, IMongoDatabase db)
    {
        if (!collections.Contains("orders"))
            await db.CreateCollectionAsync("orders");

        var ordersCollection = db.GetCollection<Order>("orders");

        var orderIdIndexKey = Builders<Order>.IndexKeys
            .Ascending(x => x.OrderId);

        var orderIdIndex = new CreateIndexModel<Order>(orderIdIndexKey, new()
        {
            Unique = true
        });

        var externalIdChanelIndexKeys = Builders<Order>.IndexKeys
            .Ascending(x => x.ExternalReferenceId)
            .Ascending(x => x.OriginChannel);

        var externalIdChanelIndex = new CreateIndexModel<Order>(externalIdChanelIndexKeys, new()
        {
            Unique = true
        });

        await ordersCollection.Indexes.CreateManyAsync([orderIdIndex, externalIdChanelIndex]);
    }

    private static async Task SetupEventsCollection(List<string> collections, IMongoDatabase db)
    {
        if (!collections.Contains("events"))
            await db.CreateCollectionAsync("events");

        var ordersCollection = db.GetCollection<Event>("events");

        var eventIdOrderIdIndexKeys = Builders<Event>.IndexKeys
            .Ascending(x => x.EventId)
            .Ascending(x => x.OrderId);

        var eventIdOrderIdIndex = new CreateIndexModel<Event>(eventIdOrderIdIndexKeys, new()
        {
            Unique = true
        });

        var orderIdIndexKey = Builders<Event>.IndexKeys
            .Ascending(x => x.OrderId);

        var orderIdIndex = new CreateIndexModel<Event>(orderIdIndexKey, new());

        await ordersCollection.Indexes.CreateManyAsync([eventIdOrderIdIndex, orderIdIndex]);
    }
}
