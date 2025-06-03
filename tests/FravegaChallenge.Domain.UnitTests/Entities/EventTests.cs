using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;

namespace FravegaChallenge.Domain.UnitTests.Entities;

public class EventTests
{
    private const int ValidOrderId = 10;
    private const string ValidEventId = "evt-123";
    private static readonly EventType ValidType = EventType.PaymentReceived;
    private const string ValidUser = "alice@example.com";

    [Test]
    [TestCase(0)]
    [TestCase(-5)]
    public void Create_OrderIdLessThanOne_ReturnsInvalidOrderIdError(int invalidOrderId)
    {
        // Act
        var result = Event.Create(
            orderId: invalidOrderId,
            id: ValidEventId,
            type: ValidType,
            date: DateTime.UtcNow,
            user: ValidUser);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Event.InvalidOrderId);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_IdNullOrWhiteSpace_ReturnsIdEmptyError(string? invalidId)
    {
        // Act
        var result = Event.Create(
            orderId: ValidOrderId,
            id: invalidId!,
            type: ValidType,
            date: DateTime.UtcNow,
            user: ValidUser);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Event.IdEmpty);
    }

    [Test]
    public void Create_UserEmptyString_ReturnsUserEmptyError()
    {
        // Act
        var result = Event.Create(
            orderId: ValidOrderId,
            id: ValidEventId,
            type: ValidType,
            date: DateTime.UtcNow,
            user: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Event.UserEmpty);
    }

    [Test]
    public void Create_UserNull_IsAllowed_ReturnsSuccessWithNullUser()
    {
        // Arrange
        string? user = null;
        var date = DateTime.UtcNow;
        // Act
        var result = Event.Create(
            orderId: ValidOrderId,
            id: ValidEventId,
            type: ValidType,
            date: date,
            user: user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var ev = result.Value;
        ev.Should().NotBeNull();
        ev.OrderId.Should().Be(ValidOrderId);
        ev.EventId.Should().Be(ValidEventId);
        ev.Type.Should().Be(ValidType);
        ev.Date.Should().Be(date);
        ev.User.Should().BeNullOrEmpty();
    }

    [Test]
    public void Create_AllValidParameters_ReturnsSuccessAndProperEvent()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var result = Event.Create(
            orderId: ValidOrderId,
            id: ValidEventId,
            type: ValidType,
            date: date,
            user: ValidUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var ev = result.Value;
        ev.Should().NotBeNull();
        ev.OrderId.Should().Be(ValidOrderId);
        ev.EventId.Should().Be(ValidEventId);
        ev.Type.Should().Be(ValidType);
        ev.Date.Should().Be(date);
        ev.User.Should().Be(ValidUser);
    }
}
