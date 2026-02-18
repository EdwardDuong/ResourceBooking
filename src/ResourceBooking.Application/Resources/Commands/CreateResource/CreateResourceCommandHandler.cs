using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Resources.Commands.CreateResource;

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Guid>
{
    private readonly IResourceRepository _resourceRepository;

    public CreateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Guid> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = new Resource(request.Name, request.Description);
        await _resourceRepository.AddAsync(resource, cancellationToken);
        return resource.Id;
    }
}
