using MediatR;

namespace ResourceBooking.Application.Bookings.Queries.GetAvailability;

public record GetAvailabilityQuery(Guid ResourceId, DateOnly Date) : IRequest<ResourceAvailabilityDto>;
