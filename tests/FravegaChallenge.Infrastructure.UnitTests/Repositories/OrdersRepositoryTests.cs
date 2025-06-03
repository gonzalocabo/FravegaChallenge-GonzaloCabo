using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
using FravegaChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace FravegaChallenge.Infrastructure.UnitTests.Repositories;

public class OrdersRepositoryTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<Order>> _collectionMock = null!;
    private Mock<ILogger<OrdersRepository>> _loggerMock = null!;
    private OrdersRepository _repository = null!;

    private Buyer _validBuyer = null!;
    private Product _validProduct = null!;
    private Order _sampleOrder = null!;

    private const int TestOrderId = 100;

    [SetUp]
    public void SetUp()
    {
        _collectionMock = new Mock<IMongoCollection<Order>>();
        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _mongoDatabaseMock
            .Setup(db => db.GetCollection<Order>(
                It.Is<string>(s => s == "orders"),
                It.IsAny<MongoCollectionSettings>()))
            .Returns(_collectionMock.Object);

        _loggerMock = new Mock<ILogger<OrdersRepository>>();

        _repository = new OrdersRepository(
            mongoDatabase: _mongoDatabaseMock.Object,
            logger: _loggerMock.Object);

        var buyerResult = Buyer.Create(
            firstName: "Juan",
            lastName: "Pérez",
            documentNumber: "12345678",
            phone: "+541112345678");
        _validBuyer = buyerResult.Value;

        var productResult = Product.Create(
            sku: "P001",
            name: "Producto A",
            description: "Descripción",
            price: 50m,
            quantity: 2);
        _validProduct = productResult.Value;

        var orderResult = Order.Create(
            id: TestOrderId,
            externalReferenceId: "EXT-100",
            originChannel: OriginChannel.Ecommerce,
            purchaseDate: DateTime.UtcNow,
            totalValue: _validProduct.Price * _validProduct.Quantity,
            buyer: _validBuyer,
            products: [_validProduct]);
        _sampleOrder = orderResult.Value;
    }

    #region GetAsync Tests

    [Test]
    public async Task GetAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        // Mock de IAsyncCursor<Order> para devolver _sampleOrder
        var cursorMock = new Mock<IAsyncCursor<Order>>();
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(c => c.Current)
            .Returns([_sampleOrder]);

        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<FindOptions<Order, Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _repository.GetAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(_sampleOrder);
    }

    [Test]
    public async Task GetAsync_WhenMongoThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var simulatedException = new Exception("Mongo FindAsync error");
        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<FindOptions<Order, Order>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.GetAsync(TestOrderId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.OrdersRepository.GetAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString() == InfrastructureErrors.OrdersRepository.GetAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region GetByFiltersAsync Tests

    [Test]
    public async Task GetByFiltersAsync_WhenOrdersMatchFilter_ReturnsListOfOrders()
    {
        // Arrange
        var expectedList = new List<Order> { _sampleOrder, _sampleOrder };

        var cursorMock = new Mock<IAsyncCursor<Order>>();
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(c => c.Current)
            .Returns(expectedList);

        Expression<Func<Order, bool>> anyFilter = o => o.TotalValue > 0m;

        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<FindOptions<Order, Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _repository.GetByFiltersAsync(anyFilter);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task GetByFiltersAsync_WhenMongoThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        Expression<Func<Order, bool>> filter = o => o.Status == OrderStatus.Created;
        var simulatedException = new Exception("Mongo FindAsync error (filter)");
        _collectionMock
            .Setup(col => col.FindAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<FindOptions<Order, Order>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.GetByFiltersAsync(filter);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.OrdersRepository.GetByFiltersAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString() == InfrastructureErrors.OrdersRepository.GetByFiltersAsyncGenericError),
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
                It.IsAny<Order>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveAsync(_sampleOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveAsync_WhenDuplicateKey_ReturnsDuplicatedOrderError()
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
                It.IsAny<Order>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(duplicateException);

        // Act
        var result = await _repository.SaveAsync(_sampleOrder);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.OrdersRepository.DuplicatedOrderError);

        _loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never,
            "No debe loguear nada en caso de clave duplicada");
    }

    [Test]
    public async Task SaveAsync_WhenGenericException_LogsErrorAndReturnsGenericSaveError()
    {
        // Arrange
        var simulatedException = new Exception("Mongo insert error");
        _collectionMock
            .Setup(col => col.InsertOneAsync(
                It.IsAny<Order>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.SaveAsync(_sampleOrder);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.OrdersRepository.SaveAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString() == InfrastructureErrors.OrdersRepository.SaveAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_WhenReplaceSucceeds_ReturnsSuccess()
    {
        // Arrange
        _collectionMock
            .Setup(col => col.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<Order>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, _sampleOrder.OrderId));

        // Act
        var result = await _repository.UpdateAsync(_sampleOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WhenMongoThrowsException_LogsErrorAndReturnsGenericUpdateError()
    {
        // Arrange
        var simulatedException = new Exception("Mongo replace error");
        _collectionMock
            .Setup(col => col.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<Order>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(simulatedException);

        // Act
        var result = await _repository.UpdateAsync(_sampleOrder);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(InfrastructureErrors.OrdersRepository.UpdateAsyncGenericError);

        _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString() == InfrastructureErrors.OrdersRepository.UpdateAsyncGenericError),
                simulatedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}
