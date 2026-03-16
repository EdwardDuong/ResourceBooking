namespace ResourceBooking.Api.Contracts;

/// <summary>
/// The client-facing shape for creating a booking - deliberately excludes
/// a user id. RequestedByUserId comes from the authenticated principal in
/// BookingsController, never from the request body, so one user can't book
/// on another's behalf by editing the payload.
/// </summary>
public record CreateBookingRequest(Guid ResourceId, DateTimeOffset SlotStart);
