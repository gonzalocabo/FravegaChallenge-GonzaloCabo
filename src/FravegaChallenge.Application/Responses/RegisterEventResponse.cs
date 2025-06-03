using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Application.Responses;

public class RegisterEventResponse
{
    public int OrderId { get; set; }
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime UpdatedOn { get; set; }
}
