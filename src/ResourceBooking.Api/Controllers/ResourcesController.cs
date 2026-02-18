using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Application.Resources;
using ResourceBooking.Application.Resources.Commands.CreateResource;
using ResourceBooking.Application.Resources.Commands.DeactivateResource;
using ResourceBooking.Application.Resources.Queries.GetActiveResources;
using ResourceBooking.Application.Resources.Queries.GetResourceById;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly ISender _sender;

    public ResourcesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetActive(CancellationToken cancellationToken)
    {
        var resources = await _sender.Send(new GetActiveResourcesQuery(), cancellationToken);
        return Ok(resources);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _sender.Send(new GetResourceByIdQuery(id), cancellationToken);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateResourceCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateResourceCommand(id), cancellationToken);
        return NoContent();
    }
}
