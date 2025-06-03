using FravegaChallenge.Application.Interfaces;
using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using MapsterMapper;

namespace FravegaChallenge.Application.Orders;

internal class OrdersService(IOrdersRepository ordersRepository, IEventsRepository eventsRepository, ICountersRepository countersRepository, IMapper mapper) : IOrdersService
{
    public async Task<Result<CreateOrderResponse>> CreateOrder(CreateOrderRequest createOrderRequest, CancellationToken cancellationToken)
    {
        var buyer = Buyer.Create(createOrderRequest.Buyer.FirstName, createOrderRequest.Buyer.LastName, createOrderRequest.Buyer.DocumentNumber, createOrderRequest.Buyer.Phone);
        
        if (!buyer.IsSuccess)
            return ResultExtensions.Map(buyer);
        
        var products = createOrderRequest.Products.Select(x => Product.Create(x.Sku, x.Name, x.Description, x.Price, x.Quantity));
        
        if (products.Any(x => !x.IsSuccess))
            return ResultExtensions.Map(products.First(x => !x.IsSuccess));
        
        var newIdResult = await countersRepository.GetNextSequenceValue("orders");

        if (!newIdResult.IsSuccess)
            return ResultExtensions.Map(newIdResult);

        var order = Order.Create(
            newIdResult.Value,
            createOrderRequest.ExternalReferenceId,
            createOrderRequest.Channel,
            createOrderRequest.PurchaseDate,
            createOrderRequest.TotalValue,
            buyer,
            products.Select(x => x.Value).ToArray());

        if (!order.IsSuccess)
            return ResultExtensions.Map(order);

        var result = await ordersRepository.SaveAsync(order.Value, cancellationToken);

        if(!result.IsSuccess)
            return ResultExtensions.Map(result);

        return Result.Success(new CreateOrderResponse()
        {
            OrderId = order.Value.OrderId,
            Status = order.Value.Status,
            UpdatedOn = order.Value.UpdatedOn
        });
    }

    public async Task<Result<GetOrderResponse>> GetOrder(int orderId, CancellationToken cancellationToken)
    {
        var orderResult = await ordersRepository.GetAsync(orderId, cancellationToken);

        if (!orderResult.IsSuccess)
            return ResultExtensions.Map(orderResult);

        var eventsResult = await eventsRepository.GetByOrderIdAsync(orderId, cancellationToken);

        GetOrderEvent[]? eventsResponse = null;
        if(eventsResult.IsSuccess)
        {
            eventsResponse = mapper.Map<GetOrderEvent[]>(eventsResult.Value);
        }

        var response = mapper.Map<GetOrderResponse>(orderResult.Value);

        response.Events = eventsResponse ?? [];

        return Result.Success(response);
    }

    public async Task<Result<GetOrderResponse[]>> SearchOrders(SearchOrderFilters filters, CancellationToken cancellationToken)
    {

        var ordersResult = await ordersRepository.GetByFiltersAsync(filters.GetExpression(), cancellationToken);
        
        if (!ordersResult.IsSuccess)
            return ResultExtensions.Map(ordersResult);
        
        var response = mapper.Map<GetOrderResponse[]>(ordersResult.Value);

        var tasks = response.Select(async x =>
        {
            var @event = await eventsRepository.GetLastByOrderIdAsync(x.OrderId, cancellationToken);
            if (@event.IsSuccess)
                x.Events = [mapper.Map<GetOrderEvent>(@event.Value)];
        });

        await Task.WhenAll(tasks);

        return Result.Success(response);
    }
}
