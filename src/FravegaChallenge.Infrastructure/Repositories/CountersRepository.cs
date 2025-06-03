using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FravegaChallenge.Infrastructure.Repositories;

internal sealed class CountersRepository(IMongoDatabase mongoDatabase, ILogger<CountersRepository> logger) : ICountersRepository
{
    private readonly IMongoCollection<BsonDocument> _counterCollection = mongoDatabase.GetCollection<BsonDocument>("counters");
    public async Task<Result<int>> GetNextSequenceValue(string counterName)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", counterName);
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);

            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var result = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);

            return Result.Success(result["seq"].AsInt32);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, InfrastructureErrors.CountersRepository.GetNextSequenceValueGenericError);
            return Result.Error(InfrastructureErrors.CountersRepository.GetNextSequenceValueGenericError);
        }
    }
}
