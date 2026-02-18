using MediatR;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Application.Resources.Queries.GetActiveResources;

public class GetActiveResourcesQueryHandler
    : IRequestHandler<GetActiveResourcesQuery, IReadOnlyList<ResourceDto>>
{
    private readonly IResourceRepository _resourceRepository;

    public GetActiveResourcesQueryHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<IReadOnlyList<ResourceDto>> Handle(
        GetActiveResourcesQuery request, CancellationToken cancellationToken)
    {
        var resources = await _resourceRepository.GetActiveAsync(cancellationToken);
        return resources.Select(r => r.ToDto()).ToList();
    }
}
