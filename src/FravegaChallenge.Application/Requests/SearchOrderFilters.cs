using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
namespace FravegaChallenge.Application.Requests;

public class SearchOrderFilters
{
    public int? OrderId { get; set; }
    public string? DocumentNumber { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? CreatedOnFrom { get; set; }
    public DateTime? CreatedOnTo { get; set; }

    public System.Linq.Expressions.Expression<Func<Order, bool>> GetExpression()
    {
        return (Order order) =>
        (true
          && (OrderId.HasValue ? order.OrderId == OrderId.Value : true)
          && (!string.IsNullOrEmpty(DocumentNumber) ? order.Buyer.DocumentNumber == DocumentNumber : true)
          && (Status.HasValue ? order.Status == Status.Value : true)
          && (CreatedOnFrom.HasValue ? order.PurchaseDate >= CreatedOnFrom.Value.ToUniversalTime() : true)
          && (CreatedOnTo.HasValue ? order.PurchaseDate <= CreatedOnTo.Value.ToUniversalTime() : true)
        );
    }
}
