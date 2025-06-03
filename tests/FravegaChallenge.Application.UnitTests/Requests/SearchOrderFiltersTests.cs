using FravegaChallenge.Application.Requests;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Application.UnitTests.Requests;

public class SearchOrderFiltersTests
{
    private Product _product = null!;

    [SetUp]
    public void SetUp()
    {
        var productResult = Product.Create("SKU1", "Prod", "Desc", 10m, 1);
        _product = productResult.Value;
    }

    private Order CreateOrder(
        int orderId,
        string documentNumber,
        OrderStatus status,
        DateTime purchaseDateUtc)
    {
        var buyer = Buyer.Create("First", "Last", documentNumber, "0000").Value;

        var orderResult = Order.Create(
            orderId,
            $"EXT-{orderId}",
            OriginChannel.Ecommerce,
            purchaseDateUtc,
            _product.Price * _product.Quantity,
            buyer,
            [_product]);

        var order = orderResult.Value;

        if (status != OrderStatus.Created)
        {
            var eventType = status switch
            {
                OrderStatus.PaymentReceived => EventType.PaymentReceived,
                OrderStatus.Invoiced => EventType.PaymentReceived,
                OrderStatus.Returned => EventType.PaymentReceived,
                _ => EventType.PaymentReceived
            };
            var evt = Event.Create(orderId, $"EVT-{orderId}", eventType, DateTime.UtcNow, "tester").Value;
            order.Update(evt);

            if (status == OrderStatus.Invoiced || status == OrderStatus.Returned)
            {
                var nextEventType = status == OrderStatus.Invoiced
                    ? EventType.Invoiced
                    : EventType.Returned;
                var nextEvt = Event.Create(orderId, $"EVT2-{orderId}", nextEventType, DateTime.UtcNow, "tester").Value;
                order.Update(nextEvt);
            }
        }

        return order;
    }

    [Test]
    public void GetExpression_NoFilters_AllowsAllOrders()
    {
        // Arrange
        var filters = new SearchOrderFilters();
        var expr = filters.GetExpression().Compile();

        var order1 = CreateOrder(1, "DOC123", OrderStatus.Created, DateTime.UtcNow.AddHours(-10));
        var order2 = CreateOrder(2, "DOC999", OrderStatus.PaymentReceived, DateTime.UtcNow.AddHours(-5));
        var order3 = CreateOrder(3, "DOC000", OrderStatus.Invoiced, DateTime.UtcNow);

        // Act & Assert
        expr(order1).Should().BeTrue();
        expr(order2).Should().BeTrue();
        expr(order3).Should().BeTrue();
    }

    [Test]
    public void GetExpression_FilterByOrderId_MatchesOnlyThatOrder()
    {
        // Arrange
        var filters = new SearchOrderFilters { OrderId = 2 };
        var expr = filters.GetExpression().Compile();

        var order1 = CreateOrder(1, "DOC123", OrderStatus.Created, DateTime.UtcNow);
        var order2 = CreateOrder(2, "DOC999", OrderStatus.Created, DateTime.UtcNow);

        // Act & Assert
        expr(order1).Should().BeFalse();
        expr(order2).Should().BeTrue();
    }

    [Test]
    public void GetExpression_FilterByDocumentNumber_MatchesBuyerDocument()
    {
        // Arrange
        var filters = new SearchOrderFilters { DocumentNumber = "DOC999" };
        var expr = filters.GetExpression().Compile();

        var orderA = CreateOrder(1, "DOC123", OrderStatus.Created, DateTime.UtcNow);
        var orderB = CreateOrder(2, "DOC999", OrderStatus.Created, DateTime.UtcNow);

        // Act & Assert
        expr(orderA).Should().BeFalse();
        expr(orderB).Should().BeTrue();
    }

    [Test]
    public void GetExpression_FilterByStatus_MatchesCorrectStatus()
    {
        // Arrange
        var filters = new SearchOrderFilters { Status = OrderStatus.Invoiced };
        var expr = filters.GetExpression().Compile();

        var orderCreated = CreateOrder(1, "DOC", OrderStatus.Created, DateTime.UtcNow);
        var orderInvoiced = CreateOrder(2, "DOC", OrderStatus.Invoiced, DateTime.UtcNow);

        // Act & Assert
        expr(orderCreated).Should().BeFalse();
        expr(orderInvoiced).Should().BeTrue();
    }
}
