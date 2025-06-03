using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FravegaChallenge.Infrastructure.Repositories;

internal sealed class EventsRepository(IMongoDatabase mongoDatabase, ILogger<EventsRepository> logger) : IEventsRepository
{
    private readonly IMongoCollection<Event> _eventsCollection = mongoDatabase.GetCollection<Event>("events");

    public async Task<Result<List<Event>>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Event>.Filter
                .Eq(x => x.OrderId, orderId);

            var events = await (await _eventsCollection.FindAsync(filter, cancellationToken: cancellationToken)).ToListAsync(cancellationToken);

            return Result.Success(events);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.EventsRepository.GetByOrderIdAsyncGenericError);
            return Result.Error(InfrastructureErrors.EventsRepository.GetByOrderIdAsyncGenericError);
        }
    }

    public async Task<Result<Event>> GetLastByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Event>.Filter
                .Eq(x => x.OrderId, orderId);

            var sort = Builders<Event>.Sort
                .Descending(x => x.Date);

            var @event = await (await _eventsCollection.FindAsync(filter, new()
            {
                Limit = 1,
                Sort = sort
            }, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);

            return Result.Success(@event);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.EventsRepository.GetLastByOrderIdAsyncGenericError);
            return Result.Error(InfrastructureErrors.EventsRepository.GetLastByOrderIdAsyncGenericError);
        }
    }

    public async Task<Result> SaveAsync(Event @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _eventsCollection.InsertOneAsync(@event, new()
            {
                BypassDocumentValidation = false
            }, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            if (ex is MongoWriteException mongoWriteEx && mongoWriteEx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                return Result.Error(InfrastructureErrors.EventsRepository.DuplicatedId);

            logger.LogError(ex, InfrastructureErrors.EventsRepository.SaveAsyncGenericError);
            return Result.Error(InfrastructureErrors.EventsRepository.SaveAsyncGenericError);
        }
    }
}
