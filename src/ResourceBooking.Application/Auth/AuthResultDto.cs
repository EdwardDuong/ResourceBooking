using ResourceBooking.Domain.Enums;

namespace ResourceBooking.Application.Auth;

public record AuthResultDto(string Token, Guid UserId, string Email, UserRole Role);
