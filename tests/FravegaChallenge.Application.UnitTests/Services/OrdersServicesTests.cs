using Ardalis.Result;
using FravegaChallenge.Application.Orders;
using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using MapsterMapper;
using System.Linq.Expressions;

namespace FravegaChallenge.Application.UnitTests.Services;

public class OrdersServicesTests
{
    private Mock<IOrdersRepository> _ordersRepoMock = null!;
    private Mock<IEventsRepository> _eventsRepoMock = null!;
    private Mock<ICountersRepository> _countersRepoMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private OrdersService _service = null!;

    private const int TestOrderId = 555;
    private CreateOrderRequest _validCreateRequest = null!;
    private Order _validOrder = null!;
    private Event _validEvent = null!;

    [SetUp]
    public void SetUp()
    {
        _ordersRepoMock = new Mock<IOrdersRepository>();
        _eventsRepoMock = new Mock<IEventsRepository>();
        _countersRepoMock = new Mock<ICountersRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new OrdersService(
            ordersRepository: _ordersRepoMock.Object,
            eventsRepository: _eventsRepoMock.Object,
            countersRepository: _countersRepoMock.Object,
            mapper: _mapperMock.Object);

        _validCreateRequest = new CreateOrderRequest
        {
            Buyer = new CreateOrderBuyer
            {
                FirstName = "Juan",
                LastName = "Pérez",
                DocumentNumber = "12345678",
                Phone = "+541112345678"
            },
            Products =
            [
                new CreateOrderProduct
                {
                    Sku = "P001",
                    Name = "Producto A",
                    Description = "Descripción",
                    Price = 10m,
                    Quantity = 2
                },
                new CreateOrderProduct
                {
                    Sku = "P002",
                    Name = "Producto B",
                    Description = "Descripción",
                    Price = 20m,
                    Quantity = 1
                }
            ],
            ExternalReferenceId = "EXT-001",
            Channel = OriginChannel.Ecommerce,
            PurchaseDate = DateTime.UtcNow,
            TotalValue = 10m * 2 + 20m * 1
        };

        var buyer = Buyer.Create(
            _validCreateRequest.Buyer.FirstName,
            _validCreateRequest.Buyer.LastName,
            _validCreateRequest.Buyer.DocumentNumber,
            _validCreateRequest.Buyer.Phone).Value;

        var product1 = Product.Create(
            _validCreateRequest.Products[0].Sku,
            _validCreateRequest.Products[0].Name,
            _validCreateRequest.Products[0].Description,
            _validCreateRequest.Products[0].Price,
            _validCreateRequest.Products[0].Quantity).Value;
        var product2 = Product.Create(
            _validCreateRequest.Products[1].Sku,
            _validCreateRequest.Products[1].Name,
            _validCreateRequest.Products[1].Description,
            _validCreateRequest.Products[1].Price,
            _validCreateRequest.Products[1].Quantity).Value;

        var orderResult = Order.Create(
            TestOrderId,
            _validCreateRequest.ExternalReferenceId,
            _validCreateRequest.Channel,
            _validCreateRequest.PurchaseDate,
            _validCreateRequest.TotalValue,
            buyer,
            [product1, product2]);
        _validOrder = orderResult.Value;

        // Crear un Event de ejemplo
        var eventResult = Event.Create(
            orderId: TestOrderId,
            id: "EVT-001",
            type: EventType.PaymentReceived,
            date: DateTime.UtcNow,
            user: "user@example.com");
        _validEvent = eventResult.Value;
    }

    #region CreateOrder Tests

    [Test]
    public async Task CreateOrder_WhenBuyerInvalid_ReturnsMappedFailure()
    {
        // Arrange
        var badRequest = new CreateOrderRequest
        {
            Buyer = new CreateOrderBuyer
            {
                FirstName = "", // Invalido
                LastName = "Last",
                DocumentNumber = "DOC",
                Phone = "123"
            },
            Products = _validCreateRequest.Products,
            ExternalReferenceId = "EXT-001",
            Channel = OriginChannel.Ecommerce,
            PurchaseDate = DateTime.UtcNow,
            TotalValue = 40m
        };

        // Act
        var result = await _service.CreateOrder(badRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Buyer.FirstNameEmpty);
    }

    [Test]
    public async Task CreateOrder_WhenAnyProductInvalid_ReturnsMappedFailure()
    {
        // Arrange
        var badRequest = new CreateOrderRequest
        {
            Buyer = _validCreateRequest.Buyer,
            Products =
            [
                new CreateOrderProduct
                {
                    Sku = "SKU1",
                    Name = "Name1",
                    Description = "Desc1",
                    Price = 0m, // Precio invalido
                    Quantity = 1
                }
            ],
            ExternalReferenceId = "EXT-001",
            Channel = OriginChannel.Ecommerce,
            PurchaseDate = DateTime.UtcNow,
            TotalValue = 0m
        };

        // Act
        var result = await _service.CreateOrder(badRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Product.ProductPriceLessOrEquals0);
    }

    [Test]
    public async Task CreateOrder_WhenGetNextSequenceValueFails_ReturnsMappedFailure()
    {
        // Arrange
        _countersRepoMock
            .Setup(r => r.GetNextSequenceValue("orders"))
            .ReturnsAsync(Result.Error("Counter error"));

        // Act
        var result = await _service.CreateOrder(_validCreateRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Counter error");
    }

    [Test]
    public async Task CreateOrder_WhenOrderCreateFails_ReturnsMappedFailure()
    {
        // Arrange
        var badRequest = new CreateOrderRequest
        {
            Buyer = _validCreateRequest.Buyer,
            Products = _validCreateRequest.Products,
            ExternalReferenceId = "EXT-001",
            Channel = OriginChannel.Ecommerce,
            PurchaseDate = DateTime.UtcNow,
            TotalValue = 999m // Monto que no coincide con suma de productos (40m)
        };

        _countersRepoMock
            .Setup(r => r.GetNextSequenceValue("orders"))
            .ReturnsAsync(Result.Success(TestOrderId));

        // Act
        var result = await _service.CreateOrder(badRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Orders.OrderValueMismatchProducts);
    }

    [Test]
    public async Task CreateOrder_WhenSaveFails_ReturnsMappedFailure()
    {
        // Arrange
        _countersRepoMock
            .Setup(r => r.GetNextSequenceValue("orders"))
            .ReturnsAsync(Result.Success(TestOrderId));
        _ordersRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error("DB save error"));

        // Act
        var result = await _service.CreateOrder(_validCreateRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("DB save error");
    }

    [Test]
    public async Task CreateOrder_WhenAllValid_ReturnsCreateOrderResponse()
    {
        // Arrange
        _countersRepoMock
            .Setup(r => r.GetNextSequenceValue("orders"))
            .ReturnsAsync(Result.Success(TestOrderId));
        _ordersRepoMock
            .Setup(r => r.SaveAsync(It.Is<Order>(o => o.OrderId == TestOrderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.CreateOrder(_validCreateRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.OrderId.Should().Be(TestOrderId);
        response.Status.Should().Be(OrderStatus.Created);
        response.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region GetOrder Tests

    [Test]
    public async Task GetOrder_WhenGetAsyncFails_ReturnsMappedFailure()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Domain.DomainErrors.Orders.InvalidId));

        // Act
        var result = await _service.GetOrder(TestOrderId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Orders.InvalidId);
    }

    [Test]
    public async Task GetOrder_WhenEventsRepositoryFails_MapsOrderAndReturnsEmptyEvents()
    {
        // Arrange
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));
        _eventsRepoMock
            .Setup(r => r.GetByOrderIdAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Infrastructure.InfrastructureErrors.EventsRepository.GetByOrderIdAsyncGenericError));

        var mappedResponse = new GetOrderResponse
        {
            OrderId = _validOrder.OrderId,
            ExternalReferenceId = _validOrder.ExternalReferenceId,
            Status = _validOrder.Status,
            UpdatedOn = _validOrder.UpdatedOn,
            Channel = _validOrder.OriginChannel,
            ChannelTranslate = "channel_translation",
            PurchaseDate = _validOrder.PurchaseDate,
            StatusTranslate = "status_translation",
            TotalValue = _validOrder.TotalValue,
            Buyer = new()
            {
                DocumentNumber = _validOrder.Buyer.DocumentNumber,
                FirstName = _validOrder.Buyer.FirstName,
                LastName = _validOrder.Buyer.LastName,
                Phone = _validOrder.Buyer.Phone
            },
            Products = [new() {
                Description = _validOrder.Products[0].Description,
                Name = _validOrder.Products[0].Name,
                Price = _validOrder.Products[0].Price,
                Quantity = _validOrder.Products[0].Quantity,
                SKU = _validOrder.Products[0].SKU,
            }],
            Events = Array.Empty<GetOrderEvent>()
        };

        _mapperMock
            .Setup(m => m.Map<GetOrderResponse>(_validOrder))
            .Returns(mappedResponse);

        // Act
        var result = await _service.GetOrder(TestOrderId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Channel.Should().Be(_validOrder.OriginChannel);
        response.OrderId.Should().Be(_validOrder.OrderId);
        response.PurchaseDate.Should().Be(_validOrder.PurchaseDate);
        response.TotalValue.Should().Be(_validOrder.TotalValue);
        response.UpdatedOn.Should().Be(_validOrder.UpdatedOn);
        response.ChannelTranslate.Should().Be("channel_translation");
        response.StatusTranslate.Should().Be("status_translation");
        response.Buyer.Should().NotBeNull();
        response.Buyer.DocumentNumber.Should().Be(_validOrder.Buyer.DocumentNumber);
        response.Buyer.FirstName.Should().Be(_validOrder.Buyer.FirstName);
        response.Buyer.LastName.Should().Be(_validOrder.Buyer.LastName);
        response.Buyer.Phone.Should().Be(_validOrder.Buyer.Phone);
        response.Products.Should().ContainSingle();
        response.Products[0].Description.Should().Be(_validOrder.Products[0].Description);
        response.Products[0].Name.Should().Be(_validOrder.Products[0].Name);
        response.Products[0].Price.Should().Be(_validOrder.Products[0].Price);
        response.Products[0].Quantity.Should().Be(_validOrder.Products[0].Quantity);
        response.Products[0].SKU.Should().Be(_validOrder.Products[0].SKU);
        response.Should().BeEquivalentTo(mappedResponse);
        response.Events.Should().BeEmpty();
    }

    [Test]
    public async Task GetOrder_WhenEventsRepositorySucceeds_MapsOrderAndEvents()
    {
        // Arrange
        var sampleEvents = new List<Event> { _validEvent };
        _ordersRepoMock
            .Setup(r => r.GetAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validOrder));
        _eventsRepoMock
            .Setup(r => r.GetByOrderIdAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(sampleEvents));

        var mappedEvent = new GetOrderEvent
        {
            Id = _validEvent.EventId,
            Type = _validEvent.Type,
            Date = _validEvent.Date
        };

        _mapperMock
            .Setup(m => m.Map<GetOrderEvent[]>(sampleEvents))
            .Returns([mappedEvent]);

        var mappedResponse = new GetOrderResponse
        {
            OrderId = _validOrder.OrderId,
            ExternalReferenceId = _validOrder.ExternalReferenceId,
            Status = _validOrder.Status,
            UpdatedOn = _validOrder.UpdatedOn,
            Events = [] // Se debe sobreescribir
        };

        _mapperMock
            .Setup(m => m.Map<GetOrderResponse>(_validOrder))
            .Returns(mappedResponse);

        // Act
        var result = await _service.GetOrder(TestOrderId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.OrderId.Should().Be(mappedResponse.OrderId);
        response.ExternalReferenceId.Should().Be(mappedResponse.ExternalReferenceId);
        response.Status.Should().Be(mappedResponse.Status);
        response.UpdatedOn.Should().Be(mappedResponse.UpdatedOn);
        response.Events.Should().ContainSingle();
        response.Events[0].Id.Should().Be(mappedEvent.Id);
        response.Events[0].Date.Should().Be(mappedEvent.Date);
        response.Events[0].Type.Should().Be(mappedEvent.Type);
    }

    #endregion

    #region SearchOrders Tests

    [Test]
    public async Task SearchOrders_WhenGetByFiltersFails_ReturnsMappedFailure()
    {
        // Arrange
        var filters = new SearchOrderFilters { OrderId = TestOrderId };
        _ordersRepoMock
            .Setup(r => r.GetByFiltersAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Domain.DomainErrors.Orders.InvalidId));

        // Act
        var result = await _service.SearchOrders(filters, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Domain.DomainErrors.Orders.InvalidId);
    }

    [Test]
    public async Task SearchOrders_WhenNoEvents_ReturnsMappedOrdersWithEmptyEvents()
    {
        // Arrange
        var sampleOrders = new List<Order> { _validOrder };
        var filters = new SearchOrderFilters { OrderId = TestOrderId };

        _ordersRepoMock
            .Setup(r => r.GetByFiltersAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(sampleOrders));

        _eventsRepoMock
            .Setup(r => r.GetLastByOrderIdAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Infrastructure.InfrastructureErrors.EventsRepository.GetLastByOrderIdAsyncGenericError));

        var mappedResponseArray = new[]
        {
            new GetOrderResponse
            {
                OrderId = _validOrder.OrderId,
                ExternalReferenceId = _validOrder.ExternalReferenceId,
                Status = _validOrder.Status,
                UpdatedOn = _validOrder.UpdatedOn,
                Events = Array.Empty<GetOrderEvent>()
            }
        };

        _mapperMock
            .Setup(m => m.Map<GetOrderResponse[]>(sampleOrders))
            .Returns(mappedResponseArray);

        // Act
        var result = await _service.SearchOrders(filters, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var responseArray = result.Value;
        responseArray.Should().HaveCount(1);
        responseArray[0].OrderId.Should().Be(_validOrder.OrderId);
        responseArray[0].Events.Should().BeEmpty();
    }

    [Test]
    public async Task SearchOrders_WhenEventsExist_MapsOrdersAndIncludesLastEvent()
    {
        // Arrange
        var sampleOrders = new List<Order> { _validOrder };
        var filters = new SearchOrderFilters { OrderId = TestOrderId };

        _ordersRepoMock
            .Setup(r => r.GetByFiltersAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(sampleOrders));

        _eventsRepoMock
            .Setup(r => r.GetLastByOrderIdAsync(TestOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_validEvent));

        var mappedEvent = new GetOrderEvent
        {
            Id = _validEvent.EventId,
            Type = _validEvent.Type,
            Date = _validEvent.Date
        };
        _mapperMock
            .Setup(m => m.Map<GetOrderEvent>(_validEvent))
            .Returns(mappedEvent);

        var mappedResponseArray = new[]
        {
            new GetOrderResponse
            {
                OrderId = _validOrder.OrderId,
                ExternalReferenceId = _validOrder.ExternalReferenceId,
                Status = _validOrder.Status,
                UpdatedOn = _validOrder.UpdatedOn,
                Events = [] // Se debe sobreescribir
            }
        };
        _mapperMock
            .Setup(m => m.Map<GetOrderResponse[]>(sampleOrders))
            .Returns(mappedResponseArray);

        // Act
        var result = await _service.SearchOrders(filters, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var responseArray = result.Value;
        responseArray.Should().HaveCount(1);
        var response = responseArray[0];
        response.OrderId.Should().Be(_validOrder.OrderId);
        response.Events.Should().ContainSingle().Which.Should().BeEquivalentTo(mappedEvent);
    }

    #endregion
}
