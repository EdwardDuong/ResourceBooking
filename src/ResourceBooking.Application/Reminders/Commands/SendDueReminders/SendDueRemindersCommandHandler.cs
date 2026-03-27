using MediatR;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Application.Reminders.Commands.SendDueReminders;

public class SendDueRemindersCommandHandler : IRequestHandler<SendDueRemindersCommand, int>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationSender _notificationSender;

    public SendDueRemindersCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        INotificationSender notificationSender)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _notificationSender = notificationSender;
    }

    public async Task<int> Handle(SendDueRemindersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var dueBookings = await _bookingRepository.GetBookingsDueForReminderAsync(
            now, now.Add(request.ReminderWindow), cancellationToken);

        var sentCount = 0;
        foreach (var booking in dueBookings)
        {
            var user = await _userRepository.GetByIdAsync(booking.RequestedByUserId, cancellationToken);
            if (user is null)
            {
                // Shouldn't happen - RequestedByUserId is set at booking
                // creation from an authenticated principal - but a dangling
                // reference here shouldn't take down the rest of the batch.
                continue;
            }

            await _notificationSender.SendBookingReminderAsync(
                user.Email, booking.Id, booking.SlotStart, cancellationToken);

            booking.MarkReminderSent();
            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            sentCount++;
        }

        return sentCount;
    }
}
