using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Application.Responses;

public record CreateOrderResponse
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime UpdatedOn { get; set; }
    
}
