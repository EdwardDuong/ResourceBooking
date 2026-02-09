namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised when a booking is attempted against a deactivated resource.
/// </summary>
public sealed class ResourceInactiveException : DomainException
{
    public ResourceInactiveException(Guid resourceId)
        : base($"Resource '{resourceId}' is not active and cannot be booked.")
    {
    }
}
