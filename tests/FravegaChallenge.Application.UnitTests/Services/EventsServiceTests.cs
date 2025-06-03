using Ardalis.Result;
using FravegaChallenge.Application.Interfaces;
using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Services;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;

namespace FravegaChallenge.Application.UnitTests.Services;

public class EventsServiceTests
{
    private Mock<IOrdersRepository> _ordersRepoMock = null!;
    private Mock<IEventsRepository> _eventsRepoMock = null!;
    private IEventsService _service = null!;

    private const int TestOrderId = 123;
    private Order _validOrder = null!;
    private RegisterEventRequest _validRequest = null!;

    [SetUp]
    public void SetUp()
    {
        _ordersRepoMock = new Mock<IOrdersRepository>();
        _eventsRepoMock = new Mock<IEventsRepository>();

        _service = new EventsService(
            ordersRepository: _ordersRepoMock.Object,
            eventsRepository: _eventsRepoMock.Object);

        var buyerResult = Buyer.Create(
            firstName: "Juan",
            lastName: "Pérez",
            documentNumber: "12345678",
            phone: "+541112345678");
        var buyer = buyerResult.Value;

        var productResult = Product.Create(
            sku: "P001",
            name: "Producto A",
            description: "Descripción",
            price: 50m,
            quantity: 2);
        var product = productResult.Value;

        var orderResult = Order.Create(
            id: TestOrderId,
            externalReferenceId: "EXT123",
            originChannel: OriginChannel.Ecommerce,
            purchaseDate: DateTime.UtcNow,
            totalValue: product.Price * product.Quantity,
            buyer: buyer,
            products: [product]);
        _validOrder = orderResult.Value;

        _validRequest = new RegisterEventRequest
        {
            Id = "EVT1",
            Type = EventType.PaymentReceived,
            Date = DateTime.UtcNow,
            User = "user@example.com"
        };
    }

    [Test]
    public async Task RegisterEvent_WhenGetAsyncFails_ReturnsMappedFailure()
    {
        // Arrange
        var getError = Domain.DomainErrors.Orders.InvalidId;
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(getError));

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: _validRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(getError);
    }

    [Test]
    public async Task RegisterEvent_WhenEventCreateFails_ReturnsMappedFailure()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));

        var badRequest = new RegisterEventRequest
        {
            Id = "",
            Type = EventType.PaymentReceived,
            Date = DateTime.UtcNow,
            User = "user@example.com"
        };

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: badRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Event.IdEmpty);
    }

    [Test]
    public async Task RegisterEvent_WhenUpdateOrderFails_ReturnsMappedFailure()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));

        var badTransitionRequest = new RegisterEventRequest
        {
            Id = "EVT2",
            Type = EventType.Invoiced, // No permitido desde Created
            Date = DateTime.UtcNow,
            User = "user@example.com"
        };

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: badTransitionRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Orders.InvalidEventType);
    }

    [Test]
    public async Task RegisterEvent_WhenSaveEventReturnsDuplicateKey_ReturnsSuccessWithoutUpdatingOrder()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));

        _eventsRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Infrastructure.InfrastructureErrors.EventsRepository.DuplicatedId));

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: _validRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verificamos que no se llame a UpdateAsync sobre ordersRepository
        _ordersRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RegisterEvent_WhenSaveEventFailsWithOtherError_ReturnsMappedFailure()
    {
        // Arrange
        var getOrder = Result.Success(_validOrder);
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(getOrder);

        var saveError = Infrastructure.InfrastructureErrors.EventsRepository.SaveAsyncGenericError;
        _eventsRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(saveError));

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: _validRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(saveError);
        _ordersRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RegisterEvent_WhenUpdateOrderSaveFails_ReturnsMappedFailure()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));

        // eventsRepo.SaveAsync exitoso
        _eventsRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // ordersRepo.UpdateAsync falla
        var updateError = Infrastructure.InfrastructureErrors.OrdersRepository.UpdateAsyncGenericError;
        _ordersRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(updateError));

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: _validRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(updateError);
    }

    [Test]
    public async Task RegisterEvent_WhenAllStepsSucceed_ReturnsRegisterEventResponse()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));

        // eventsRepo.SaveAsync exitoso
        _eventsRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // ordersRepo.UpdateAsync exitoso
        _ordersRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.RegisterEvent(
            orderId: TestOrderId,
            registerEventRequest: _validRequest,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.OrderId.Should().Be(_validOrder.OrderId);

        response.NewStatus.Should().Be(OrderStatus.PaymentReceived);
        response.PreviousStatus.Should().Be(OrderStatus.Created);
        response.UpdatedOn.Should().BeCloseTo(_validOrder.UpdatedOn, TimeSpan.FromSeconds(1));
    }

}
