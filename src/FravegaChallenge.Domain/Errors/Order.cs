namespace FravegaChallenge.Domain;

public partial class DomainErrors
{
    public static class Orders
    {
        public static string InvalidId => "Id must be a valid value.";
        public static string MismatchId => "Event order id does not match with order id.";
        public static string BuyerEmpty => "Buyer can not be null.";
        public static string ProductsEmpty => "Products can not be null or empty.";
        public static string ExternalReferenceIdEmpty => "ExternalReferenceId must contain a value.";
        public static string PurchaseDateNonUtc => "PurchaseDate must be a UTC value.";
        public static string OrderValueMismatchProducts => "TotalValue does not match product's values.";
        public static string NextStatusUnavailable => "Order has no next status available.";
        public static string InvalidEventType => "Event type is invalid for the order status.";
    }
}
