using FravegaChallenge.Domain.Entities;

namespace FravegaChallenge.Infrastructure.Repositories.Abstractions;

public interface IEventsRepository
{
    /// <summary>
    /// Get all events of the given order id.
    /// </summary>
    /// <param name="orderId">The order Id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of <see cref="List{Z}"/> of (<see cref="Event"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="List{Z}"/> of <see cref="Event"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<List<Event>>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get last event of the given order id.
    /// </summary>
    /// <param name="orderId">The order Id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Event"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Event"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<Event>> GetLastByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new event.
    /// </summary>
    /// <param name="event">The event to be saved</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/>.
    /// If successful, status will be <see cref="ResultStatus.Ok"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result> SaveAsync(Event @event, CancellationToken cancellationToken = default);
}
