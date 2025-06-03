namespace FravegaChallenge.Domain.Entities;

public class Buyer
{
    private Buyer()
    { }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;

    /// <summary>
    /// Factory method to create a new <see cref="Buyer"/> with the given parameters.
    /// </summary>
    /// <param name="firstName">Buyer's first name. Must not be empty or null.</param>
    /// <param name="lastName">Buyer's last name. Must not be empty or null.</param>
    /// <param name="documentNumber">Buyer's document number. Must not be empty or null.</param>
    /// <param name="phone">Buyer's phone. Must not be empty or null.</param>
    /// <returns>
    /// Returns a <see cref="Result{T}"/> of (<see cref="Buyer"/>).
    /// If successful, status will be <see cref="ResultStatus.Ok"/> with the instance of <see cref="Buyer"/> in <see cref="Result{T}.Value"/>.
    /// Otherwise, it will be <see cref="ResultStatus.Error"/> with error in <see cref="Result{T}.Errors"/>.
    /// </returns>
    public static Result<Buyer> Create(string firstName, string lastName, string documentNumber, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Error(DomainErrors.Buyer.FirstNameEmpty);
        
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Error(DomainErrors.Buyer.LastNameEmpty);
        
        if (string.IsNullOrWhiteSpace(documentNumber))
            return Result.Error(DomainErrors.Buyer.DocumentNumberEmpty);
        
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Error(DomainErrors.Buyer.PhoneEmpty);

        return new Buyer
        {
            FirstName = firstName,
            LastName = lastName,
            DocumentNumber = documentNumber,
            Phone = phone
        };
    }
}
