namespace FravegaChallenge.Infrastructure;
public partial class InfrastructureErrors
{
    public static class OrdersRepository
    {
        public static string GetAsyncGenericError => "Error attempting to retrieve order.";
        public static string GetByFiltersAsyncGenericError => "Error attempting to retrieve orders.";
        public static string DuplicatedOrderError => "An order with same ExternalReferenceId and Channel already exists.";
        public static string SaveAsyncGenericError => "Error attempting to create new order.";
        public static string UpdateAsyncGenericError => "Error attempting to update order.";
    }
}
