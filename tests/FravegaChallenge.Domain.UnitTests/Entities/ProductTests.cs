using FravegaChallenge.Domain.Entities;
namespace FravegaChallenge.Domain.UnitTests.Entities;

public class ProductTests
{
    private const string ValidSku = "P001";
    private const string ValidName = "Producto A";
    private const string ValidDescription = "Descripción";
    private const decimal ValidPrice = 100.50m;
    private const int ValidQuantity = 5;

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceSku_ReturnsSkuEmptyError(string? invalidSku)
    {
        // Act
        var result = Product.Create(
            sku: invalidSku!,
            name: ValidName,
            description: ValidDescription,
            price: ValidPrice,
            quantity: ValidQuantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Product.SkuEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceName_ReturnsNameEmptyError(string? invalidName)
    {
        // Act
        var result = Product.Create(
            sku: ValidSku,
            name: invalidName!,
            description: ValidDescription,
            price: ValidPrice,
            quantity: ValidQuantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Product.NameEmpty);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrWhiteSpaceDescription_ReturnsDescriptionEmptyError(string? invalidDescription)
    {
        // Act
        var result = Product.Create(
            sku: ValidSku,
            name: ValidName,
            description: invalidDescription!,
            price: ValidPrice,
            quantity: ValidQuantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Product.DescriptionEmpty);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Create_WithPriceLessOrEqualZero_ReturnsProductPriceError(decimal invalidPrice)
    {
        // Act
        var result = Product.Create(
            sku: ValidSku,
            name: ValidName,
            description: ValidDescription,
            price: invalidPrice,
            quantity: ValidQuantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Product.ProductPriceLessOrEquals0);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-3)]
    public void Create_WithQuantityLessOrEqualZero_ReturnsProductQuantityError(int invalidQuantity)
    {
        // Act
        var result = Product.Create(
            sku: ValidSku,
            name: ValidName,
            description: ValidDescription,
            price: ValidPrice,
            quantity: invalidQuantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Product.ProductQuantityLessOrEquals0);
    }

    [Test]
    public void Create_WithAllValidParameters_ReturnsSuccessAndProperProduct()
    {
        // Act
        var result = Product.Create(
            sku: ValidSku,
            name: ValidName,
            description: ValidDescription,
            price: ValidPrice,
            quantity: ValidQuantity);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var product = result.Value;
        product.Should().NotBeNull();
        product.SKU.Should().Be(ValidSku);
        product.Name.Should().Be(ValidName);
        product.Description.Should().Be(ValidDescription);
        product.Price.Should().Be(ValidPrice);
        product.Quantity.Should().Be(ValidQuantity);
    }
}
