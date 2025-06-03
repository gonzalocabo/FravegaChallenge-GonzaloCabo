namespace FravegaChallenge.Domain;
public partial class DomainErrors
{
    public static class Product
    {
        public static string SkuEmpty => "Product's sku can not be null or empty.";
        public static string NameEmpty => "Product's name can not be null or empty.";
        public static string DescriptionEmpty => "Product's description can not be null or empty.";
        public static string ProductPriceLessOrEquals0 => "Product's price must be greater than 0.";
        public static string ProductQuantityLessOrEquals0 => "Product's quantity must be greater than 0.";
    }
}
