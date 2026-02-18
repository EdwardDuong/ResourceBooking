using MediatR;

namespace ResourceBooking.Application.Resources.Commands.DeactivateResource;

public record DeactivateResourceCommand(Guid ResourceId) : IRequest;
