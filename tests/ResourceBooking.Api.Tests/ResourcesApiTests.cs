using System.Net;
using System.Net.Http.Json;
using ResourceBooking.Application.Resources;
using ResourceBooking.Application.Resources.Commands.CreateResource;
using Xunit;

namespace ResourceBooking.Api.Tests;

public class ResourcesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResourcesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidName_Returns201AndAppearsInActiveList()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/resources", new CreateResourceCommand("Conference Room A", "3rd floor"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<Guid>();

        var listResponse = await _client.GetAsync("/api/resources");
        var resources = await listResponse.Content.ReadFromJsonAsync<List<ResourceDto>>();

        Assert.Contains(resources!, r => r.Id == id && r.Name == "Conference Room A");
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/resources", new CreateResourceCommand(string.Empty, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/resources/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_RemovesResourceFromActiveList()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/resources", new CreateResourceCommand("Room To Deactivate", null));
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/resources/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/resources");
        var resources = await listResponse.Content.ReadFromJsonAsync<List<ResourceDto>>();

        Assert.DoesNotContain(resources!, r => r.Id == id);
    }
}
