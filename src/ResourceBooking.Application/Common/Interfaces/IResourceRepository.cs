using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Common.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken cancellationToken);
    Task AddAsync(Resource resource, CancellationToken cancellationToken);

    /// <summary>
    /// Persists changes made to a Resource previously loaded via
    /// GetByIdAsync (e.g. Deactivate/Reactivate).
    /// </summary>
    Task UpdateAsync(Resource resource, CancellationToken cancellationToken);
}
