using FravegaChallenge.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FravegaChallenge.Domain.Entities;

public class Event
{
    private Event() 
    { }

    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = string.Empty;
    public int OrderId { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public EventType Type { get; private set; }
    public DateTime Date { get; private set; }
    public string? User { get; private set; } = string.Empty;

    /// <summary>
    /// Factory method to create a new <see cref="Event"/> with the given parameters.
    /// </summary>
    /// <param name="orderId">The orderId affected by this event. Must be greater than 0.</param>
    /// <param name="id">The event id. Must not be empty or null.</param>
    /// <param name="type">The event type.</param>
    /// <param name="user">The user who create the event. It could be null but not empty.</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Event"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Event"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    public static Result<Event> Create(int orderId, string id, EventType type, DateTime date, string? user)
    {
        if (orderId < 1)
            return Result.Error(DomainErrors.Event.InvalidOrderId);

        if (string.IsNullOrWhiteSpace(id))
            return Result.Error(DomainErrors.Event.IdEmpty);

        if (user == string.Empty)
            return Result.Error(DomainErrors.Event.UserEmpty);

        return new Event
        {
            OrderId = orderId,
            EventId = id,
            Type = type,
            Date = date,
            User = user
        };
    }
}
