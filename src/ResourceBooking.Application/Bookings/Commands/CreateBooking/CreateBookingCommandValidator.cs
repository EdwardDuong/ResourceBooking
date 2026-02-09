using FluentValidation;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(c => c.ResourceId)
            .NotEmpty();

        RuleFor(c => c.RequestedByUserId)
            .NotEmpty();

        RuleFor(c => c.SlotStart)
            .Must(BeAlignedToSlotGrid)
            .WithMessage($"Slot start must align to {TimeSlot.Duration.TotalMinutes}-minute boundaries.")
            .Must(BeInTheFuture)
            .WithMessage("Slot start must be in the future.");
    }

    private static bool BeAlignedToSlotGrid(DateTimeOffset start) =>
        start.Minute % (int)TimeSlot.Duration.TotalMinutes == 0
        && start.Second == 0
        && start.Millisecond == 0;

    private static bool BeInTheFuture(DateTimeOffset start) => start > DateTimeOffset.UtcNow;
}
