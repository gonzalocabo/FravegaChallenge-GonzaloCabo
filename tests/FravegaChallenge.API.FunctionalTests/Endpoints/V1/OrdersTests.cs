namespace FravegaChallenge.API.FunctionalTests.Endpoints.V1;

using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Testing;

public class OrdersTests
{
    [Test]
    public async Task POST_CreateOrder_InvalidModel_ReturnsBadRequest()
    {
        var invalidPayload = new { ExternalReferenceId = "EXT123" };
        var response = await GetClient().PostAsJsonAsync("/api/v1/orders", invalidPayload);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task POST_CreateOrder_ValidRequest_ReturnsOkAndResponse()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            Buyer = new CreateOrderBuyer
            {
                FirstName = "Juan",
                LastName = "Pérez",
                DocumentNumber = "DOC123",
                Phone = "+541112345678"
            },
            Products =
            [
                new CreateOrderProduct
                {
                    Sku = "P001",
                    Name = "Producto A",
                    Description = "Descripción",
                    Price = 1000m,
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
            TotalValue = 20m + 1000m * 2
        };

        // Act
        var response = await GetClient().PostAsJsonAsync("/api/v1/orders", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        var created = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(jsonOptions);
        created.Should().NotBeNull();
        created!.OrderId.Should().Be(43);
        created.Status.Should().Be(OrderStatus.Created);
        created.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task POST_RegisterEvent_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var testOrderId = 1;

        // Act
        var response = await GetClient().PostAsJsonAsync($"/api/v1/orders/{testOrderId}/events", new { });

        // Arrange
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task POST_RegisterEvent_ValidRequest_ReturnsOk()
    {
        // Arrange
        var registerRequest = new RegisterEventRequest
        {
            Id = "EVT-XYZ",
            Type = EventType.PaymentReceived,
            Date = DateTime.UtcNow,
            User = "user@example.com"
        };

        // Act
        var response = await GetClient().PostAsJsonAsync($"/api/v1/orders/{1}/events", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        var respObj = await response.Content.ReadFromJsonAsync<RegisterEventResponse>(jsonOptions);
        respObj.Should().NotBeNull();
        respObj!.OrderId.Should().Be(1);
        respObj.NewStatus.Should().Be(OrderStatus.PaymentReceived);
        respObj.PreviousStatus.Should().Be(OrderStatus.Created);
        respObj.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GET_GetOrder_ReturnsOrderWithEvents()
    {
        // Act
        var response = await GetClient().GetAsync($"/api/v1/orders/{1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        var respObj = await response.Content.ReadFromJsonAsync<GetOrderResponse>(jsonOptions);
        respObj.Should().NotBeNull();
        respObj!.OrderId.Should().Be(1);
    }

    [Test]
    public async Task GET_SearchOrders_ReturnsOrdersWithLastEvent()
    {
        // Act
        var response = await GetClient().GetAsync($"/api/v1/orders/search?orderId={1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        var respArray = await response.Content.ReadFromJsonAsync<GetOrderResponse[]>(jsonOptions);
        respArray.Should().NotBeNull();
        respArray!.Length.Should().Be(1);
        respArray[0].OrderId.Should().Be(1);
        respArray[0].Events.Should().ContainSingle();
    }
}
