using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Infrastructure.Persistence;
using ResourceBooking.Infrastructure.Persistence.Repositories;

namespace ResourceBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
