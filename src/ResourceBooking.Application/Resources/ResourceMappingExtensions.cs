using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Resources;

public static class ResourceMappingExtensions
{
    public static ResourceDto ToDto(this Resource resource) =>
        new(resource.Id, resource.Name, resource.Description, resource.IsActive, resource.CreatedAtUtc);
}
