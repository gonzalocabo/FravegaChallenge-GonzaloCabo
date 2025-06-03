using FravegaChallenge.API.Endpoints.V1.Descriptions;
using FravegaChallenge.Application.Interfaces;
using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Threading;

namespace FravegaChallenge.API.Endpoints.V1;

public class OrdersEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routes = app.GetRouteGroupBuilder(this);
        
        routes
            .MapPost("", CreateOrder)
            .WithName(nameof(CreateOrder))
            .WithSummary("Create a new order")
            .WithDescription(OrdersDescriptions.CREATE_ORDER_DESCRIPTION)
            .Accepts<CreateOrderRequest>("application/json")
            .Produces<IEnumerable<string>>((int)HttpStatusCode.BadRequest)
            .Produces<CreateOrderResponse>((int)HttpStatusCode.OK, "application/json")
            .WithOpenApi();

        routes
            .MapPost("{orderId}/events", RegisterEvent)
            .WithName(nameof(RegisterEvent))
            .WithSummary("Register a new event")
            .WithDescription(OrdersDescriptions.REGISTER_EVENT_DESCRIPTION)
            .Accepts<RegisterEventRequest>("application/json")
            .Produces<IEnumerable<string>>((int)HttpStatusCode.BadRequest)
            .Produces<RegisterEventResponse>((int)HttpStatusCode.OK)
            .WithOpenApi();

        routes
            .MapGet("{orderId}", GetOrder)
            .WithName(nameof(GetOrder))
            .WithSummary("Get an order")
            .WithDescription(OrdersDescriptions.GET_ORDER_DESCRIPTION)
            .Produces<IEnumerable<string>>((int)HttpStatusCode.BadRequest)
            .Produces<GetOrderResponse>((int)HttpStatusCode.OK)
            .WithOpenApi();

        routes
            .MapGet("search", SearchOrders)
            .WithName(nameof(SearchOrders))
            .WithSummary("Searchs orders")
            .WithDescription(OrdersDescriptions.SEARCH_ORDERS_DESCRIPTION)
            .Produces<IEnumerable<string>>((int)HttpStatusCode.BadRequest)
            .Produces<GetOrderResponse[]>((int)HttpStatusCode.OK)
            .WithOpenApi();
    }

    public static async Task<Results<Ok<CreateOrderResponse>, BadRequest<IEnumerable<string>>>> CreateOrder(CreateOrderRequest createOrderRequest, 
        IOrdersService ordersService, CancellationToken cancellationToken)
    {
        var result = await ordersService.CreateOrder(createOrderRequest, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Errors);
    }

    public static async Task<Results<Ok<RegisterEventResponse>, BadRequest<IEnumerable<string>>>> RegisterEvent(int orderId, RegisterEventRequest registerEventRequest,
        IEventsService eventsService, CancellationToken cancellationToken)
    {
        var result = await eventsService.RegisterEvent(orderId, registerEventRequest, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Errors);
    }

    public static async Task<Results<Ok<GetOrderResponse>, BadRequest<IEnumerable<string>>>> GetOrder(int orderId, 
        IOrdersService ordersService, CancellationToken cancellationToken)
    {
        var result = await ordersService.GetOrder(orderId, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Errors);
    }

    public static async Task<Results<Ok<GetOrderResponse[]>, BadRequest<IEnumerable<string>>>> SearchOrders(int? orderId, string? documentNumber, OrderStatus? status, DateTime? createdOnFrom, DateTime? createdOnTo,
        IOrdersService ordersService, CancellationToken cancellationToken)
    {
        var filters = new SearchOrderFilters
        {
            CreatedOnFrom = createdOnFrom,
            CreatedOnTo = createdOnTo,
            DocumentNumber = documentNumber,
            OrderId = orderId,
            Status = status
        };

        var result = await ordersService.SearchOrders(filters, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Errors);
    }
}
