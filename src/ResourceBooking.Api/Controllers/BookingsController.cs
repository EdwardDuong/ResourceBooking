using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Application.Bookings.Commands.CancelBooking;
using ResourceBooking.Application.Bookings.Commands.CreateBooking;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;

namespace ResourceBooking.Api.Controllers;

[ApiController]
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
        [FromBody] CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return Created($"/api/bookings/{id}", id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelBookingCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ResourceAvailabilityDto>> GetAvailability(
        [FromQuery] Guid resourceId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var availability = await _sender.Send(new GetAvailabilityQuery(resourceId, date), cancellationToken);
        return Ok(availability);
    }
}
