using DnsClient.Internal;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace FravegaChallenge.Infrastructure.Repositories;

internal sealed class OrdersRepository(IMongoDatabase mongoDatabase, ILogger<OrdersRepository> logger) : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection = mongoDatabase.GetCollection<Order>("orders");
    public async Task<Result<Order>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Order>.Filter
                .Eq(x => x.OrderId, id);

            var order = await (await _ordersCollection.FindAsync(filter, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);

            return Result.Success(order);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.OrdersRepository.GetAsyncGenericError);
            return Result.Error(InfrastructureErrors.OrdersRepository.GetAsyncGenericError);
        }
    }
    public async Task<Result<List<Order>>> GetByFiltersAsync(Expression<Func<Order, bool>> expressions, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await (await _ordersCollection.FindAsync(expressions, cancellationToken: cancellationToken)).ToListAsync(cancellationToken);

            return Result.Success(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.OrdersRepository.GetByFiltersAsyncGenericError);
            return Result.Error(InfrastructureErrors.OrdersRepository.GetByFiltersAsyncGenericError);
        }
    }

    public async Task<Result> SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _ordersCollection.InsertOneAsync(order, new()
            {
                BypassDocumentValidation = false
            }, cancellationToken);

            return Result.Success();
        }
        catch(Exception ex)
        {
            if(ex is MongoWriteException mongoWriteEx && mongoWriteEx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                return Result.Error(InfrastructureErrors.OrdersRepository.DuplicatedOrderError);

            logger.LogError(ex, InfrastructureErrors.OrdersRepository.SaveAsyncGenericError);
            return Result.Error(InfrastructureErrors.OrdersRepository.SaveAsyncGenericError);
        }
    }

    public async Task<Result> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Order>.Filter
                .Eq(x => x.OrderId, order.OrderId);

            await _ordersCollection.ReplaceOneAsync(filter, order, new ReplaceOptions()
            {
                BypassDocumentValidation = false,
                IsUpsert = false
            }, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.OrdersRepository.UpdateAsyncGenericError);
            return Result.Error(InfrastructureErrors.OrdersRepository.UpdateAsyncGenericError);
        }
    }

}
