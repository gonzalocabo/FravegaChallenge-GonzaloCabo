using FravegaChallenge.Domain.Enums;
using System.Text.Json.Serialization;

namespace FravegaChallenge.Application.Responses;

public class GetOrderResponse
{
    public int OrderId { get; set; }
    public string ExternalReferenceId { get; set; } = string.Empty;
    public OriginChannel Channel { get; set; }
    [JsonPropertyName("channel_translate")]
    public string ChannelTranslate { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalValue { get; set; }
    public GetOrderBuyer Buyer { get; set; } = new();
    public GetOrderProduct[] Products { get; set; } = [];
    public OrderStatus Status { get; set; }
    [JsonPropertyName("status_translate")]
    public string StatusTranslate { get; set; } = string.Empty;
    public DateTime UpdatedOn { get; set; }
    public GetOrderEvent[] Events { get; set; } = [];
}

public class GetOrderBuyer
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class GetOrderProduct
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class GetOrderEvent
{
    public string Id { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public DateTime Date { get; set; }
}