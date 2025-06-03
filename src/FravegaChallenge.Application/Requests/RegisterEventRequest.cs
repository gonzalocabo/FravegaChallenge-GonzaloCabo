using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Application.Requests;

public class RegisterEventRequest
{
    public string Id { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public DateTime Date { get; set; }
    public string? User { get; set; }
}
