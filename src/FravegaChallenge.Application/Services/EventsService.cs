using FravegaChallenge.Application.Interfaces;
using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;

namespace FravegaChallenge.Application.Services;

internal class EventsService(IOrdersRepository ordersRepository, IEventsRepository eventsRepository) : IEventsService
{
    public async Task<Result<RegisterEventResponse>> RegisterEvent(int orderId, RegisterEventRequest registerEventRequest, CancellationToken cancellationToken)
    {
        var orderResult = await ordersRepository.GetAsync(orderId, cancellationToken);
        if (!orderResult.IsSuccess)
            return ResultExtensions.Map(orderResult);

        var newEvent = Event.Create(orderResult.Value.OrderId, registerEventRequest.Id, registerEventRequest.Type, registerEventRequest.Date, registerEventRequest.User);

        if (!newEvent.IsSuccess)
            return ResultExtensions.Map(newEvent);

        var updateOrderResult = orderResult.Value.Update(newEvent);

        if(!updateOrderResult.IsSuccess)
            return ResultExtensions.Map(updateOrderResult);

        var saveEventResult = await eventsRepository.SaveAsync(newEvent, cancellationToken);

        if (!saveEventResult.IsSuccess)
        {
            if (saveEventResult.Errors.First() == Infrastructure.InfrastructureErrors.EventsRepository.DuplicatedId)
                return Result.Success();

            return ResultExtensions.Map(saveEventResult);
        }

        var saveOrderResult = await ordersRepository.UpdateAsync(orderResult.Value);

        if(!saveOrderResult.IsSuccess)
            return ResultExtensions.Map(saveOrderResult);

        return Result.Success(new RegisterEventResponse
        {
            OrderId = orderResult.Value.OrderId,
            NewStatus = orderResult.Value.Status,
            PreviousStatus = updateOrderResult.Value,
            UpdatedOn = orderResult.Value.UpdatedOn
        });
    }
}
