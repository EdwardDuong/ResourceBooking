using MediatR;

namespace ResourceBooking.Application.Resources.Commands.CreateResource;

public record CreateResourceCommand(string Name, string? Description) : IRequest<Guid>;
