using FravegaChallenge.Domain.Entities;
using System.Linq.Expressions;

namespace FravegaChallenge.Infrastructure.Repositories.Abstractions;

public interface IOrdersRepository
{
    /// <summary>
    /// Get an order by the given Id.
    /// </summary>
    /// <param name="id">The order Id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Order"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Order"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<Order>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders matching the expression.
    /// </summary>
    /// <param name="expressions">Expressions to be matched</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of <see cref="List{Z}"/> of (<see cref="Order"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="List{Z}"/> of <see cref="Order"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<List<Order>>> GetByFiltersAsync(Expression<Func<Order, bool>> expressions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the order.
    /// </summary>
    /// <param name="order">Order to be updated</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/>.
    /// If successful, status will be <see cref="ResultStatus.Ok"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result> UpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new order.
    /// </summary>
    /// <param name="order">Order to be saved</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/>.
    /// If successful, status will be <see cref="ResultStatus.Ok"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result> SaveAsync(Order order, CancellationToken cancellationToken = default);
}
