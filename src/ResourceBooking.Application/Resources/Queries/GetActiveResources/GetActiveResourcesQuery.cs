using MediatR;

namespace ResourceBooking.Application.Resources.Queries.GetActiveResources;

public record GetActiveResourcesQuery : IRequest<IReadOnlyList<ResourceDto>>;
