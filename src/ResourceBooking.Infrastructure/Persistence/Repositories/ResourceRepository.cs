using Microsoft.EntityFrameworkCore;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Infrastructure.Persistence.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _dbContext;

    public ResourceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Resources.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken cancellationToken) =>
        await _dbContext.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Resource resource, CancellationToken cancellationToken)
    {
        await _dbContext.Resources.AddAsync(resource, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Resource resource, CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
