using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Common.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken cancellationToken);
    Task AddAsync(Resource resource, CancellationToken cancellationToken);
}
