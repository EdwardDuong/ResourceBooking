using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Application.Auth;
using ResourceBooking.Application.Auth.Commands.Login;
using ResourceBooking.Application.Auth.Commands.Register;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(
        [FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
