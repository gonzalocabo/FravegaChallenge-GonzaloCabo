using FravegaChallenge.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FravegaChallenge.Domain.Entities;

public class Order
{
    private Order(Buyer buyer) 
    {
        Buyer = buyer;
    }

    // Diccionario de transacciones validas.
    // Representa los estados posibles a partir de su estado actual.
    // Key = estado actual
    // Value = estados posibles
    private static readonly Dictionary<OrderStatus, List<EventType>> VALID_STATUSES_TRANSITIONS = new()
    {
        {OrderStatus.Created, [EventType.PaymentReceived, EventType.Canceled] },
        {OrderStatus.PaymentReceived, [EventType.Invoiced] },
        {OrderStatus.Invoiced, [EventType.Returned] },
    };

    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = string.Empty;

    public int OrderId { get; private set; }
    public string ExternalReferenceId { get; private set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public OriginChannel OriginChannel { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public decimal TotalValue { get; private set; }
    public Buyer Buyer { get; private set; }
    public Product[] Products { get; private set; } = [];
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; private set; }
    public DateTime UpdatedOn { get; private set; }

    /// <summary>
    /// Factory method to create a new <see cref="Order"/> with the given parameters.
    /// </summary>
    /// <param name="id">Numeric OrderId for the order. Must be greater than 0.</param>
    /// <param name="externalReferenceId">ExternalReferenceId. Must not be empty or null.</param>
    /// <param name="originChannel">Origin channel for the order.</param>
    /// <param name="purchaseDate">Purchase date. Must be of UTC kind.</param>
    /// <param name="totalValue">Total value. Must be the sum of all product prices multiplied by quantity.</param>
    /// <param name="buyer">Buyer. Must not be null.</param>
    /// <param name="products">Array of products. Must not be empty or null.</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Order"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Order"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    public static Result<Order> Create(
        int id,
        string externalReferenceId,
        OriginChannel originChannel,
        DateTime purchaseDate,
        decimal totalValue,
        Buyer buyer,
        Product[] products)
    {
        if (id < 1)
            return Result.Error(DomainErrors.Orders.InvalidId);

        if(buyer is null)
            return Result.Error(DomainErrors.Orders.BuyerEmpty);

        if(products is null || products.Count() < 1 || products.Any(x => x is null))
            return Result.Error(DomainErrors.Orders.ProductsEmpty);

        if (string.IsNullOrWhiteSpace(externalReferenceId))
            return Result.Error(DomainErrors.Orders.ExternalReferenceIdEmpty);

        if(purchaseDate.Kind != DateTimeKind.Utc)
            return Result.Error(DomainErrors.Orders.PurchaseDateNonUtc);

        if(totalValue != products.Sum(x => x.Price * x.Quantity))
            return Result.Error(DomainErrors.Orders.OrderValueMismatchProducts);

        return new Order(buyer)
        {
            OrderId = id,
            ExternalReferenceId = externalReferenceId,
            OriginChannel = originChannel,
            PurchaseDate = purchaseDate,
            TotalValue = totalValue,
            Products = products,
            Status = OrderStatus.Created,
            UpdatedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates order with given event
    /// </summary>
    /// <param name="event">The event</param>
    /// <returns>
    /// Returns a result <see cref="Result{T}"/> of (<see cref="OrderStatus"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the previous order status in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    public Result<OrderStatus> Update(Event @event)
    {
        // Mapeo entre EventType y OrderStatus
        Result<OrderStatus> MapFromEvent(EventType eventType) 
            => @event.Type switch
            {
                EventType.PaymentReceived => Result.Success(OrderStatus.PaymentReceived),
                EventType.Canceled => Result.Success(OrderStatus.Canceled),
                EventType.Invoiced => Result.Success(OrderStatus.Invoiced),
                EventType.Returned => Result.Success(OrderStatus.Returned),
                _ => Result.Error(),
            };

        if (@event.OrderId != OrderId)
            return Result.Error(DomainErrors.Orders.MismatchId);

        if (!VALID_STATUSES_TRANSITIONS.TryGetValue(Status, out var statuses))
            return Result.Error(DomainErrors.Orders.NextStatusUnavailable);
            
        if(!statuses.Contains(@event.Type))
            return Result.Error(DomainErrors.Orders.InvalidEventType);

        var mapResult = MapFromEvent(@event.Type);
        
        if (!mapResult.IsSuccess)
            return ResultExtensions.Map(mapResult);

        var previousStatus = Status;

        Status = mapResult.Value;
        UpdatedOn = DateTime.UtcNow;

        return Result.Success(previousStatus);
    }
}
