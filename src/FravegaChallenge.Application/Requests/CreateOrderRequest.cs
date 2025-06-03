using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Application.Requests;

public record CreateOrderRequest
{
    public string ExternalReferenceId { get; set; } = string.Empty;
    public OriginChannel Channel { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalValue { get; set; }
    public CreateOrderBuyer Buyer { get; set; } = new();
    public CreateOrderProduct[] Products { get; set; } = [];
}

public class CreateOrderBuyer
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class CreateOrderProduct
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
