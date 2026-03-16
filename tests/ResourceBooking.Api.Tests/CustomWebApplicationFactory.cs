using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResourceBooking.Application.Auth;
using ResourceBooking.Application.Auth.Commands.Login;
using ResourceBooking.Application.Auth.Commands.Register;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Enums;
using ResourceBooking.Infrastructure.Persistence;

namespace ResourceBooking.Api.Tests;

/// <summary>
/// Swaps the SQL Server DbContext registration for an in-memory SQLite
/// connection so the API test suite has no external dependency. The
/// connection is kept open for the lifetime of the factory - SQLite drops an
/// in-memory database as soon as its last connection closes.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    /// <summary>
    /// Returns an HttpClient with a valid bearer token attached. Admin users
    /// are seeded directly through the repository (registration always
    /// creates a Member - there's no client-facing way to self-assign the
    /// Admin role, which is the point).
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(bool isAdmin = false)
    {
        var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Password123!";

        if (isAdmin)
        {
            using var scope = Services.CreateScope();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var admin = new User(email, hasher.Hash(password), UserRole.Admin);
            await userRepository.AddAsync(admin, CancellationToken.None);
        }
        else
        {
            var registerResponse = await client.PostAsJsonAsync(
                "/api/auth/register", new RegisterCommand(email, password));
            registerResponse.EnsureSuccessStatusCode();
        }

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, password));
        loginResponse.EnsureSuccessStatusCode();
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult!.Token);
        return client;
    }
}
