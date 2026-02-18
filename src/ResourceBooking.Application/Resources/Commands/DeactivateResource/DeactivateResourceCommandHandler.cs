using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Application.Resources.Commands.DeactivateResource;

public class DeactivateResourceCommandHandler : IRequestHandler<DeactivateResourceCommand>
{
    private readonly IResourceRepository _resourceRepository;

    public DeactivateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task Handle(DeactivateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        resource.Deactivate();

        await _resourceRepository.UpdateAsync(resource, cancellationToken);
    }
}
