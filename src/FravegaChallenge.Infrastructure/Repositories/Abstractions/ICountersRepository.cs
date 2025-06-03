namespace FravegaChallenge.Infrastructure.Repositories.Abstractions;

public interface ICountersRepository
{
    /// <summary>
    /// Gets the next sequence value for a specific counter.
    /// </summary>
    /// <param name="counterName">The counter name</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="int"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the next sequence value in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    Task<Result<int>> GetNextSequenceValue(string counterName);
}
