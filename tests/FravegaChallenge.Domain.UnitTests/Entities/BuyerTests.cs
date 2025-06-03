using FravegaChallenge.Domain.Entities;

namespace FravegaChallenge.Domain.UnitTests.Entities;

public class BuyerTests
{
    private const string ValidFirstName = "Juan";
    private const string ValidLastName = "Pérez";
    private const string ValidDocumentNumber = "12345678";
    private const string ValidPhone = "+541112345678";

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceFirstName_ReturnsFirstNameEmptyError(string? invalidFirstName)
    {
        // Act
        var result = Buyer.Create(
            firstName: invalidFirstName!,
            lastName: ValidLastName,
            documentNumber: ValidDocumentNumber,
            phone: ValidPhone);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Buyer.FirstNameEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceLastName_ReturnsLastNameEmptyError(string? invalidLastName)
    {
        // Act
        var result = Buyer.Create(
            firstName: ValidFirstName,
            lastName: invalidLastName!,
            documentNumber: ValidDocumentNumber,
            phone: ValidPhone);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Buyer.LastNameEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceDocumentNumber_ReturnsDocumentNumberEmptyError(string? invalidDocumentNumber)
    {
        // Act
        var result = Buyer.Create(
            firstName: ValidFirstName,
            lastName: ValidLastName,
            documentNumber: invalidDocumentNumber!,
            phone: ValidPhone);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Buyer.DocumentNumberEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpacePhone_ReturnsPhoneEmptyError(string? invalidPhone)
    {
        // Act
        var result = Buyer.Create(
            firstName: ValidFirstName,
            lastName: ValidLastName,
            documentNumber: ValidDocumentNumber,
            phone: invalidPhone!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Buyer.PhoneEmpty);
    }

    [Test]
    public void Create_WithAllValidParameters_ReturnsSuccessAndProperBuyer()
    {
        // Act
        var result = Buyer.Create(
            firstName: ValidFirstName,
            lastName: ValidLastName,
            documentNumber: ValidDocumentNumber,
            phone: ValidPhone);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var buyer = result.Value;
        buyer.Should().NotBeNull();
        buyer.FirstName.Should().Be(ValidFirstName);
        buyer.LastName.Should().Be(ValidLastName);
        buyer.DocumentNumber.Should().Be(ValidDocumentNumber);
        buyer.Phone.Should().Be(ValidPhone);
    }
}
