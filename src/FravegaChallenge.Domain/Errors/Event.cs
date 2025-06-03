namespace FravegaChallenge.Domain;

public partial class DomainErrors
{
    public static class Event
    {
        public static string InvalidOrderId => "OrderId must have a valid value.";
        public static string IdEmpty => "Id must have a non null & not empty value.";
        public static string UserEmpty => "User must have a not empty or null value.";
    }
}
