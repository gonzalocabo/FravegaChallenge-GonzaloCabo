using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Domain.UnitTests.Entities;

public class OrderTests
{
    private const int ValidOrderId = 1;
    private const string ValidExternalReferenceId = "ext-001";
    private static readonly OriginChannel ValidOriginChannel = OriginChannel.Ecommerce;
    private static readonly DateTime ValidPurchaseDateUtc = DateTime.UtcNow;
    private const decimal ValidPrice = 50m;
    private const int ValidQuantity = 2;

    private Buyer _validBuyer = null!;
    private Product _validProduct = null!;

    [SetUp]
    public void SetUp()
    {
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
            price: ValidPrice,
            quantity: ValidQuantity);
        _validProduct = productResult.Value;
    }

    #region Create Factory Method Tests

    [Test]
    [TestCase(0)]
    [TestCase(-5)]
    public void Create_OrderIdLessThanOne_ReturnsInvalidIdError(int invalidId)
    {
        // Act
        var result = Order.Create(
            id: invalidId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: [_validProduct]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.InvalidId);
    }

    [Test]
    public void Create_NullBuyer_ReturnsBuyerEmptyError()
    {
        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: null!,
            products: [_validProduct]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.BuyerEmpty);
    }

    [Test]
    public void Create_NullProducts_ReturnsProductsEmptyError()
    {
        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.ProductsEmpty);
    }

    [Test]
    public void Create_EmptyProductsArray_ReturnsProductsEmptyError()
    {
        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: Array.Empty<Product>());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.ProductsEmpty);
    }

    [Test]
    public void Create_ProductsArrayContainsNull_ReturnsProductsEmptyError()
    {
        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: [_validProduct, null!]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.ProductsEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_NullOrWhiteSpaceExternalReferenceId_ReturnsExternalReferenceIdEmptyError(string? invalidExternalRef)
    {
        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: invalidExternalRef!,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: [_validProduct]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.ExternalReferenceIdEmpty);
    }

    [Test]
    public void Create_PurchaseDateNotUtc_ReturnsPurchaseDateNonUtcError()
    {
        // Arrange
        var localDate = DateTime.Now; // Kind = Local

        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: localDate,
            totalValue: ValidPrice * ValidQuantity,
            buyer: _validBuyer,
            products: [_validProduct]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.PurchaseDateNonUtc);
    }

    [Test]
    public void Create_TotalValueMismatch_ReturnsOrderValueMismatchProductsError()
    {
        // Arrange
        var wrongTotal = ValidPrice * ValidQuantity + 10m;

        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: wrongTotal,
            buyer: _validBuyer,
            products: [_validProduct]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.OrderValueMismatchProducts);
    }

    [Test]
    public void Create_AllValidParameters_ReturnsSuccessAndProperOrder()
    {
        // Arrange
        var expectedTotal = _validProduct.Price * _validProduct.Quantity;
        var before = DateTime.UtcNow;

        // Act
        var result = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: expectedTotal,
            buyer: _validBuyer,
            products: [_validProduct]);

        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        var order = result.Value;
        order.Should().NotBeNull();
        order.OrderId.Should().Be(ValidOrderId);
        order.ExternalReferenceId.Should().Be(ValidExternalReferenceId);
        order.OriginChannel.Should().Be(ValidOriginChannel);
        order.PurchaseDate.Should().Be(ValidPurchaseDateUtc);
        order.TotalValue.Should().Be(expectedTotal);
        order.Buyer.Should().BeSameAs(_validBuyer);
        order.Products.Should().ContainSingle().Which.Should().BeSameAs(_validProduct);
        order.Status.Should().Be(OrderStatus.Created);
        order.UpdatedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    #endregion

    #region Update Method Tests

    private Order _order = null!;

    [SetUp]
    public void CreateValidOrder()
    {
        var orderResult = Order.Create(
            id: ValidOrderId,
            externalReferenceId: ValidExternalReferenceId,
            originChannel: ValidOriginChannel,
            purchaseDate: ValidPurchaseDateUtc,
            totalValue: _validProduct.Price * _validProduct.Quantity,
            buyer: _validBuyer,
            products: [_validProduct]);
        orderResult.IsSuccess.Should().BeTrue();
        _order = orderResult.Value;
    }

    [Test]
    public void Update_EventOrderIdDoesNotMatchOrderId_ReturnsMismatchIdError()
    {
        //Arrange
        var differentOrderId = ValidOrderId + 1;
        var mismatchedEventResult = Event.Create(
            orderId: differentOrderId,
            id: "evt-mismatch",
            type: EventType.PaymentReceived,
            date: DateTime.UtcNow,
            user: "adminUser123");

        mismatchedEventResult.IsSuccess.Should().BeTrue();

        var mismatchedEvent = mismatchedEventResult.Value;

        // Act
        var result = _order.Update(mismatchedEvent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.MismatchId);
    }

    [Test]
    public void Update_InvalidCurrentStatus_ReturnsNextStatusUnavailableError()
    {
        // Arrange
        var cancelEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-cancel",
            type: EventType.Canceled,
            date: DateTime.UtcNow,
            user: "adminUser123");
        _order.Update(cancelEventResult.Value);

        // Act
        var invoicedEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-invoiced",
            type: EventType.Invoiced,
            date: DateTime.UtcNow,
            user: "adminUser123");
        invoicedEventResult.IsSuccess.Should().BeTrue();
        var invoicedEvent = invoicedEventResult.Value;

        var result = _order.Update(invoicedEvent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.NextStatusUnavailable);
    }

    [Test]
    public void Update_EventTypeNotAllowedForCurrentStatus_ReturnsInvalidEventTypeError()
    {
        // Arrange
        var invoicedEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-invoice",
            type: EventType.Invoiced,
            date: DateTime.UtcNow,
            user: "adminUser123");
        invoicedEventResult.IsSuccess.Should().BeTrue();
        var invoicedEvent = invoicedEventResult.Value;

        // Act
        var result = _order.Update(invoicedEvent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Orders.InvalidEventType);
    }

    [Test]
    public void Update_ValidTransitionFromCreatedToCanceled_ReturnsPreviousStatusAndUpdatesOrder()
    {
        // Arrange
        var paymentEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-payment",
            type: EventType.Canceled,
            date: DateTime.UtcNow,
            user: "adminUser123");
        paymentEventResult.IsSuccess.Should().BeTrue();
        var paymentEvent = paymentEventResult.Value;

        var before = DateTime.UtcNow;

        // Act
        var result = _order.Update(paymentEvent);

        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Created, "El valor devuelto debe ser el estado anterior");

        _order.Status.Should().Be(OrderStatus.Canceled);
        _order.UpdatedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Test]
    public void Update_ValidTransitionFromCreatedToPaymentReceived_ReturnsPreviousStatusAndUpdatesOrder()
    {
        // Arrange
        var paymentEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-payment",
            type: EventType.PaymentReceived,
            date: DateTime.UtcNow,
            user: "adminUser123");
        paymentEventResult.IsSuccess.Should().BeTrue();
        var paymentEvent = paymentEventResult.Value;

        var before = DateTime.UtcNow;

        // Act
        var result = _order.Update(paymentEvent);

        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Created, "El valor devuelto debe ser el estado anterior");

        _order.Status.Should().Be(OrderStatus.PaymentReceived);
        _order.UpdatedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Test]
    public void Update_ValidTransitionFromPaymentReceivedToInvoiced_ReturnsPreviousStatusAndUpdatesOrder()
    {
        // Arrange
        var paymentEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-payment",
            type: EventType.PaymentReceived,
            date: DateTime.UtcNow,
            user: "adminUser123");
        _order.Update(paymentEventResult.Value);

        var invoicedEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-invoice",
            type: EventType.Invoiced,
            date: DateTime.UtcNow,
            user: "adminUser123");
        invoicedEventResult.IsSuccess.Should().BeTrue();
        var invoicedEvent = invoicedEventResult.Value;

        var before = DateTime.UtcNow;

        // Act
        var result = _order.Update(invoicedEvent);

        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.PaymentReceived, "El valor devuelto debe ser el estado anterior");

        _order.Status.Should().Be(OrderStatus.Invoiced);
        _order.UpdatedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Test]
    public void Update_ValidTransitionFromInvoicedToReturned_ReturnsPreviousStatusAndUpdatesOrder()
    {
        // Arrange: Primero pasar de Created -> PaymentReceived -> Invoiced
        var paymentEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-payment",
            type: EventType.PaymentReceived,
            date: DateTime.UtcNow,
            user: "adminUser123");
        _order.Update(paymentEventResult.Value);


        var invoicedEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-invoice",
            type: EventType.Invoiced,
            date: DateTime.UtcNow,
            user: "adminUser123");
        _order.Update(invoicedEventResult.Value);
        

        var returnedEventResult = Event.Create(
            orderId: ValidOrderId,
            id: "evt-return",
            type: EventType.Returned,
            date: DateTime.UtcNow,
            user: "adminUser123");
        var returnedEvent = returnedEventResult.Value;

        var before = DateTime.UtcNow;

        // Act
        var result = _order.Update(returnedEvent);

        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Invoiced, "El valor devuelto debe ser el estado anterior");

        _order.Status.Should().Be(OrderStatus.Returned);
        _order.UpdatedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    #endregion
}
