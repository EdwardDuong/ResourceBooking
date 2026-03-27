using MediatR;

namespace ResourceBooking.Application.Reminders.Commands.SendDueReminders;

/// <summary>
/// Sends a reminder for every active booking whose slot starts within
/// ReminderWindow from now and hasn't already had one sent. Returns the
/// count sent, mainly so the background scanner has something to log.
/// </summary>
public record SendDueRemindersCommand(TimeSpan ReminderWindow) : IRequest<int>;
