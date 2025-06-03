using FravegaChallenge.Application.Requests;
using FravegaChallenge.Application.Responses;

namespace FravegaChallenge.Application.Interfaces;

public interface IEventsService
{
    /// <summary>
    /// Register a new event for an order
    /// </summary>
    /// <param name="orderId">The order Id</param>
    /// <param name="registerEventRequest">The request</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="RegisterEventResponse"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="RegisterEventResponse"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<RegisterEventResponse>> RegisterEvent(int orderId, RegisterEventRequest registerEventRequest, CancellationToken cancellationToken);
}
