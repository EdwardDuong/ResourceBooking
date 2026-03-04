using System.Net;
using System.Net.Http.Json;
using ResourceBooking.Application.Bookings.Commands.CreateBooking;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;
using ResourceBooking.Application.Resources.Commands.CreateResource;
using Xunit;

namespace ResourceBooking.Api.Tests;

public class BookingsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BookingsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ForOpenSlot_Returns201()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var response = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForAlreadyTakenSlot_Returns409()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var first = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnalignedSlot_Returns400()
    {
        var resourceId = await CreateResourceAsync();
        var unalignedSlot = NextAlignedFutureSlot().AddMinutes(1);

        var response = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), unalignedSlot));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ThenRebook_Succeeds()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));
        var bookingId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var cancelResponse = await _client.DeleteAsync($"/api/bookings/{bookingId}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var rebookResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));
        Assert.Equal(HttpStatusCode.Created, rebookResponse.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_ReflectsTakenSlot()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingCommand(resourceId, Guid.NewGuid(), slotStart));

        var date = DateOnly.FromDateTime(slotStart.UtcDateTime);
        var response = await _client.GetAsync($"/api/bookings/availability?resourceId={resourceId}&date={date:yyyy-MM-dd}");
        var availability = await response.Content.ReadFromJsonAsync<ResourceAvailabilityDto>();

        var takenSlot = availability!.Slots.Single(s => s.SlotStart == slotStart);
        Assert.False(takenSlot.IsAvailable);
    }

    private async Task<Guid> CreateResourceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/resources", new CreateResourceCommand($"Room {Guid.NewGuid()}", null));
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static DateTimeOffset NextAlignedFutureSlot()
    {
        var slotMinutes = 15;
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var alignedMinute = ((start.Minute / slotMinutes) + 1) * slotMinutes;
        return new DateTimeOffset(start.Year, start.Month, start.Day, start.Hour, 0, 0, start.Offset)
            .AddMinutes(alignedMinute);
    }
}
