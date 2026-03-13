using MediatR;

namespace ResourceBooking.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<AuthResultDto>;
