using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ResourceBooking.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Authenticated principal is missing a 'sub' claim.");

        return Guid.Parse(value);
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
}
