namespace ResourceBooking.Domain.Entities;

/// <summary>
/// A bookable resource (e.g. a meeting room, a piece of equipment, a staff member).
/// Deliberately generic: the booking rules in this system do not depend on what
/// kind of resource is being scheduled.
/// </summary>
public class Resource
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Resource() { }

    public Resource(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource name cannot be empty.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
