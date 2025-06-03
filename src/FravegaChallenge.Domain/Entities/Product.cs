namespace FravegaChallenge.Domain.Entities;

public class Product
{
    private Product() 
    { }

    public string SKU { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    /// <summary>
    /// Factory method to create a new <see cref="Product"/> with the given parameters.
    /// </summary>
    /// <param name="sku">Product´s SKU. Must not be empty or null.</param>
    /// <param name="name">Product´s name. Must not be empty or null.</param>
    /// <param name="description">Product´s description. Must not be empty or null.</param>
    /// <param name="price">Product´s price. Must be greater than 0.</param>
    /// <param name="quantity">Product´s price. Must be greater than 0.</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Product"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Product"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    public static Result<Product> Create(string sku, string name, string description, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result.Error(DomainErrors.Product.SkuEmpty);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Error(DomainErrors.Product.NameEmpty);
        
        if (string.IsNullOrWhiteSpace(description))
            return Result.Error(DomainErrors.Product.DescriptionEmpty);

        if (price <= 0)
            return Result.Error(DomainErrors.Product.ProductPriceLessOrEquals0);

        if (quantity <= 0)
            return Result.Error(DomainErrors.Product.ProductQuantityLessOrEquals0);

        return new Product
        {
            SKU = sku,
            Name = name,
            Description = description,
            Price = price,
            Quantity = quantity
        };
    }
}
