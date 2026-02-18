namespace ResourceBooking.Application.Resources;

public record ResourceDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
