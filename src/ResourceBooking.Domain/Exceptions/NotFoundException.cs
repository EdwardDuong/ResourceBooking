namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised when an operation references an entity that does not exist.
/// Mapped to 404 Not Found at the API layer.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} '{id}' was not found.")
    {
    }
}
