using System.Net;
using System.Net.Http.Json;
using ResourceBooking.Api.Contracts;
using ResourceBooking.Application.Bookings;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;
using ResourceBooking.Application.Resources.Commands.CreateResource;
using Xunit;

namespace ResourceBooking.Api.Tests;

public class BookingsApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public BookingsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync() => _client = await _factory.CreateAuthenticatedClientAsync(isAdmin: true);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ForOpenSlot_Returns201()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var response = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForAlreadyTakenSlot_Returns409()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var first = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnalignedSlot_Returns400()
    {
        var resourceId = await CreateResourceAsync();
        var unalignedSlot = NextAlignedFutureSlot().AddMinutes(1);

        var response = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, unalignedSlot));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthToken_Returns401()
    {
        var anonymousClient = _factory.CreateClient();
        var resourceId = await CreateResourceAsync();

        var response = await anonymousClient.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, NextAlignedFutureSlot()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ThenRebook_Succeeds()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        var bookingId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var cancelResponse = await _client.DeleteAsync($"/api/bookings/{bookingId}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var rebookResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        Assert.Equal(HttpStatusCode.Created, rebookResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_ByNonOwnerNonAdmin_Returns403()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        var bookingId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var otherMemberClient = await _factory.CreateAuthenticatedClientAsync();
        var cancelResponse = await otherMemberClient.DeleteAsync($"/api/bookings/{bookingId}");

        Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_ReflectsTakenSlot()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));

        var date = DateOnly.FromDateTime(slotStart.UtcDateTime);
        var response = await _client.GetAsync($"/api/bookings/availability?resourceId={resourceId}&date={date:yyyy-MM-dd}");
        var availability = await response.Content.ReadFromJsonAsync<ResourceAvailabilityDto>();

        var takenSlot = availability!.Slots.Single(s => s.SlotStart == slotStart);
        Assert.False(takenSlot.IsAvailable);
    }

    [Fact]
    public async Task GetAvailability_AfterCachedRead_ReflectsBookingMadeImmediately()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        // Populate the cache with a "this slot is open" result before booking it.
        var beforeBooking = await GetAvailabilityAsync(resourceId, slotStart);
        Assert.True(beforeBooking.Slots.Single(s => s.SlotStart == slotStart).IsAvailable);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var afterBooking = await GetAvailabilityAsync(resourceId, slotStart);
        Assert.False(afterBooking.Slots.Single(s => s.SlotStart == slotStart).IsAvailable);
    }

    [Fact]
    public async Task GetAvailability_AfterCachedRead_ReflectsCancellationImmediately()
    {
        var resourceId = await CreateResourceAsync();
        var slotStart = NextAlignedFutureSlot();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, slotStart));
        var bookingId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Populate the cache with a "this slot is taken" result before cancelling.
        var beforeCancel = await GetAvailabilityAsync(resourceId, slotStart);
        Assert.False(beforeCancel.Slots.Single(s => s.SlotStart == slotStart).IsAvailable);

        var cancelResponse = await _client.DeleteAsync($"/api/bookings/{bookingId}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var afterCancel = await GetAvailabilityAsync(resourceId, slotStart);
        Assert.True(afterCancel.Slots.Single(s => s.SlotStart == slotStart).IsAvailable);
    }

    [Fact]
    public async Task GetMine_OnlyReturnsCallersOwnBookings()
    {
        var resourceId = await CreateResourceAsync();
        var mySlot = NextAlignedFutureSlot();
        await _client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(resourceId, mySlot));

        var otherMemberClient = await _factory.CreateAuthenticatedClientAsync();
        await otherMemberClient.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(resourceId, mySlot.AddMinutes(15)));

        var response = await _client.GetAsync("/api/bookings/mine");
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingDto>>();

        Assert.Single(bookings!, b => b.SlotStart == mySlot);
    }

    private async Task<Guid> CreateResourceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/resources", new CreateResourceCommand($"Room {Guid.NewGuid()}", null));
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<ResourceAvailabilityDto> GetAvailabilityAsync(Guid resourceId, DateTimeOffset slotStart)
    {
        var date = DateOnly.FromDateTime(slotStart.UtcDateTime);
        var response = await _client.GetAsync(
            $"/api/bookings/availability?resourceId={resourceId}&date={date:yyyy-MM-dd}");
        return (await response.Content.ReadFromJsonAsync<ResourceAvailabilityDto>())!;
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
