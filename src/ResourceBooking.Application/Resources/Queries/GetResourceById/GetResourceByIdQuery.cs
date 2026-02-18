using MediatR;

namespace ResourceBooking.Application.Resources.Queries.GetResourceById;

public record GetResourceByIdQuery(Guid ResourceId) : IRequest<ResourceDto>;
