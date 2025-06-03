namespace FravegaChallenge.Domain;

public partial class DomainErrors
{
    public static class Buyer
    {
        public static string FirstNameEmpty => "Buyer's first name must not be empty.";
        public static string LastNameEmpty => "Buyer's last name must not be empty.";
        public static string DocumentNumberEmpty => "Buyer's document number must not be empty.";
        public static string PhoneEmpty => "Buyer's phone must not be empty.";
    }
}
