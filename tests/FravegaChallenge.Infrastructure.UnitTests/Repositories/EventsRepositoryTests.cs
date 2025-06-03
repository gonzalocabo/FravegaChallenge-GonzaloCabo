using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
using FravegaChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

namespace FravegaChallenge.Infrastructure.UnitTests.Repositories;

public class EventsRepositoryTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<Event>> _collectionMock = null!;
    private Mock<ILogger<EventsRepository>> _loggerMock = null!;
    private EventsRepository _repository = null!;

    private const string CollectionName = "events";
    private const int TestOrderId = 42;
    private Event _sampleEvent = null!;

    [SetUp]
    public void SetUp()
    {
        _collectionMock = new Mock<IMongoCollection<Event>>();

        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _mongoDatabaseMock
            .Setup(db => db.GetCollection<Event>(
                It.Is<string>(s => s == CollectionName),
                It.IsAny<MongoCollectionSettings>()))
            .Returns(_collectionMock.Object);

        _loggerMock = new Mock<ILogger<EventsRepository>>();

        _repository = new EventsRepository(
            mongoDatabase: _mongoDatabaseMock.Object,
            logger: _loggerMock.Object);

        var eventResult = Event.Create(
            orderId: TestOrderId,
            id: "event-001",
            type: EventType.PaymentReceived,
            date: DateTime.Now,
            user: "adminUser123");

        _sampleEvent = eventResult.Value;
    }

    #region GetByOrderIdAsync Tests

    [Test]
    public async Task GetByOrderIdAsync_WhenEventsExist_ReturnsListOfEvents()
    {
        // Arrange
        var expectedList = new List<Event> { _sampleEvent, _sampleEvent };

        var cursorMock = new Mock<IAsyncCursor<Event>>();
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(c => c.Current)
            .Returns(expectedList);

        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<FindOptions<Event, Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _repository.GetByOrderIdAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task GetByOrderIdAsync_WhenMongoThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var simulatedException = new Exception("Mongo error in FindAsync");
        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<FindOptions<Event, Event>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.GetByOrderIdAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(
            InfrastructureErrors.EventsRepository.GetByOrderIdAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString() == InfrastructureErrors.EventsRepository.GetByOrderIdAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region GetLastByOrderIdAsync Tests

    [Test]
    public async Task GetLastByOrderIdAsync_WhenEventExists_ReturnsSingleEvent()
    {
        // Arrange

        var cursorMock = new Mock<IAsyncCursor<Event>>();
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(c => c.Current)
            .Returns([_sampleEvent]);

        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.Is<FindOptions<Event, Event>>(opts => opts.Limit == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _repository.GetLastByOrderIdAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(_sampleEvent);
    }

    [Test]
    public async Task GetLastByOrderIdAsync_WhenMongoThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var simulatedException = new Exception("Mongo error in FindAsync (Last)");
        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<FindOptions<Event, Event>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.GetLastByOrderIdAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(
            InfrastructureErrors.EventsRepository.GetLastByOrderIdAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString() == InfrastructureErrors.EventsRepository.GetLastByOrderIdAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region SaveAsync Tests

    [Test]
    public async Task SaveAsync_WhenInsertSucceeds_ReturnsSuccess()
    {
        // Arrange
        _collectionMock
            .Setup(col => col.InsertOneAsync(
                It.IsAny<Event>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveAsync(_sampleEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveAsync_WhenDuplicateKey_LogsNoErrorMessageAndReturnsDuplicatedIdError()
    {
        // Arrange
        
        //Se construye por reflection WriteError y WriteConcernError debido a que sus constructores son internos
        var writeErrorType = typeof(WriteError);
        var writeErrorCtor = writeErrorType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(ServerErrorCategory), typeof(int), typeof(string), typeof(BsonDocument)],
            null)!;

        var writeError = (WriteError)writeErrorCtor.Invoke(
        [
            ServerErrorCategory.DuplicateKey,
            11000,
            "duplicate key error",
            new BsonDocument()
        ]);

        var writeConcernErrorType = typeof(WriteConcernError);
        var writeConcernErrorCtor = writeConcernErrorType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(int), typeof(string), typeof(string), typeof(BsonDocument), typeof(IEnumerable<string>)],
            null)!;
        var writeConcernError = (WriteConcernError)writeConcernErrorCtor.Invoke(
        [
            11000,
            "",
            "",
            new BsonDocument(),
            new List<string>()
        ]);
        var connectionId = new MongoDB.Driver.Core.Connections.ConnectionId(new(new(), new System.Net.DnsEndPoint("localhost", 27017)));
        var duplicateException = new MongoWriteException(connectionId, writeError, writeConcernError, new Exception());

        _collectionMock
            .Setup(col => col.InsertOneAsync(
                It.IsAny<Event>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(duplicateException);

        // Act
        var result = await _repository.SaveAsync(_sampleEvent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.EventsRepository.DuplicatedId);

        // Cuando se intente insertar un duplicado no debe loguear error.
        _loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never,
            "No debe loguear nada para excepciones de clave duplicada");
    }

    [Test]
    public async Task SaveAsync_WhenGenericException_LogsErrorAndReturnsGenericSaveError()
    {
        // Arrange
        var simulatedException = new Exception("Generic Mongo insert error");
        _collectionMock
            .Setup(col => col.InsertOneAsync(
                It.IsAny<Event>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.SaveAsync(_sampleEvent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.EventsRepository.SaveAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString() == InfrastructureErrors.EventsRepository.SaveAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}
