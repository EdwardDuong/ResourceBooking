using System.Net;
using System.Net.Http.Json;
using ResourceBooking.Application.Auth;
using ResourceBooking.Application.Auth.Commands.Login;
using ResourceBooking.Application.Auth.Commands.Register;
using Xunit;

namespace ResourceBooking.Api.Tests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsTokenAndMemberRole()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterCommand($"{Guid.NewGuid()}@example.com", "Password123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
    }

    [Fact]
    public async Task Register_WithAlreadyRegisteredEmail_Returns409()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(email, "Password123!"));

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterCommand(email, "AnotherPassword123!"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterCommand($"{Guid.NewGuid()}@example.com", "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(email, "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, "Password123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(email, "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginCommand("nobody@example.com", "Password123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
