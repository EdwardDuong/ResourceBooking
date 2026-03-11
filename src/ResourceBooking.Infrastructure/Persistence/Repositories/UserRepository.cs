using Microsoft.EntityFrameworkCore;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _dbContext.Users.SingleOrDefaultAsync(
            u => u.Email == email.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (UniqueConstraintViolationDetector.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEmailException(user.Email);
        }
    }
}
