namespace FravegaChallenge.Infrastructure;

public partial class InfrastructureErrors
{
    public static class EventsRepository
    {
        public static string GetByOrderIdAsyncGenericError => "Error attempting to retrieve events from order id.";
        public static string GetLastByOrderIdAsyncGenericError => "Error attempting to retrieve last event.";
        public static string DuplicatedId => "An event with same Id already exists.";
        public static string SaveAsyncGenericError => "Error attempting to create new event.";
    }
}
