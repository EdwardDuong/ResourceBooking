using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Common;
using ResourceBooking.Api.Contracts;
using ResourceBooking.Application.Bookings;
using ResourceBooking.Application.Bookings.Commands.CancelBooking;
using ResourceBooking.Application.Bookings.Commands.CreateBooking;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;
using ResourceBooking.Application.Bookings.Queries.GetMyBookings;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBookingCommand(request.ResourceId, User.GetUserId(), request.SlotStart);
        var id = await _sender.Send(command, cancellationToken);
        return Created($"/api/bookings/{id}", id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelBookingCommand(id, User.GetUserId(), User.IsAdmin()), cancellationToken);
        return NoContent();
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ResourceAvailabilityDto>> GetAvailability(
        [FromQuery] Guid resourceId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var availability = await _sender.Send(new GetAvailabilityQuery(resourceId, date), cancellationToken);
        return Ok(availability);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetMine(CancellationToken cancellationToken)
    {
        var bookings = await _sender.Send(new GetMyBookingsQuery(User.GetUserId()), cancellationToken);
        return Ok(bookings);
    }
}
