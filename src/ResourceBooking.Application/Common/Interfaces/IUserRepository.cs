using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Throws DuplicateEmailException if the email is already registered -
    /// see UserRepository.AddAsync.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken);
}
