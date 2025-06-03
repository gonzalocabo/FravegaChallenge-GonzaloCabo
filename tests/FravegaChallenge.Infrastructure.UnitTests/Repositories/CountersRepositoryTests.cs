using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using FravegaChallenge.Infrastructure.Repositories;

namespace FravegaChallenge.Infrastructure.UnitTests.Repositories;

public class CountersRepositoryTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<BsonDocument>> _collectionMock = null!;
    private Mock<ILogger<CountersRepository>> _loggerMock = null!;
    private CountersRepository _repository = null!;

    private const string CounterName = "myCounter";

    [SetUp]
    public void SetUp()
    {
        _collectionMock = new Mock<IMongoCollection<BsonDocument>>();

        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _mongoDatabaseMock
            .Setup(db => db.GetCollection<BsonDocument>(
                It.Is<string>(s => s == "counters"),
                It.IsAny<MongoCollectionSettings>()))
            .Returns(_collectionMock.Object);

        _loggerMock = new Mock<ILogger<CountersRepository>>();

        _repository = new CountersRepository(
            mongoDatabase: _mongoDatabaseMock.Object,
            logger: _loggerMock.Object);
    }

    [Test]
    public async Task GetNextSequenceValue_WhenCounterExists_IncrementsAndReturnsNewValue()
    {
        // Arrange
        var updatedDocument = new BsonDocument
        {
            { "_id", CounterName },
            { "seq", 43 }
        };

        _collectionMock
            .Setup(col => col.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<FindOneAndUpdateOptions<BsonDocument, BsonDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDocument);

        // Act
        var result = await _repository.GetNextSequenceValue(CounterName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(43);
    }

    [Test]
    public async Task GetNextSequenceValue_WhenMongoThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var simulatedException = new Exception("Mongo error");
        
        _collectionMock
            .Setup(col => col.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<FindOneAndUpdateOptions<BsonDocument, BsonDocument>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.GetNextSequenceValue(CounterName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(
            InfrastructureErrors.CountersRepository.GetNextSequenceValueGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == InfrastructureErrors.CountersRepository.GetNextSequenceValueGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
