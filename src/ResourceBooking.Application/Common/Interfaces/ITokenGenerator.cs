using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Common.Interfaces;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}
