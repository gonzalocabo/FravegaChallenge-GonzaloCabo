using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;

namespace FravegaChallenge.Application.Interfaces;

public interface IOrdersService
{
    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="createOrderRequest">The request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="CreateOrderResponse"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="CreateOrderResponse"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<CreateOrderResponse>> CreateOrder(CreateOrderRequest createOrderRequest, CancellationToken cancellationToken);
    
    /// <summary>
    /// Obtains an order.
    /// </summary>
    /// <param name="orderId">The order Id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="GetOrderResponse"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="GetOrderResponse"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<GetOrderResponse>> GetOrder(int orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Search for orders with the given filter.
    /// </summary>
    /// <param name="filters">The filters to apply</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of an array of (<see cref="GetOrderResponse"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of an array of <see cref="GetOrderResponse"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<GetOrderResponse[]>> SearchOrders(SearchOrderFilters filters, CancellationToken cancellationToken);
}
