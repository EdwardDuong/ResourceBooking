using ResourceBooking.Domain.Entities;
using Xunit;

namespace ResourceBooking.Domain.Tests;

public class TimeSlotTests
{
    [Fact]
    public void Constructor_WithAlignedStart_Succeeds()
    {
        var start = new DateTimeOffset(2026, 2, 4, 9, 15, 0, TimeSpan.FromHours(11));

        var slot = new TimeSlot(start);

        Assert.Equal(start, slot.Start);
        Assert.Equal(start + TimeSpan.FromMinutes(15), slot.End);
    }

    [Theory]
    [InlineData(9, 1, 0)]
    [InlineData(9, 0, 30)]
    [InlineData(9, 7, 0)]
    public void Constructor_WithUnalignedStart_Throws(int hour, int minute, int second)
    {
        var start = new DateTimeOffset(2026, 2, 4, hour, minute, second, TimeSpan.FromHours(11));

        Assert.Throws<ArgumentException>(() => new TimeSlot(start));
    }

    [Fact]
    public void Constructor_WithMillisecondComponent_Throws()
    {
        var start = new DateTimeOffset(2026, 2, 4, 9, 0, 0, 500, TimeSpan.FromHours(11));

        Assert.Throws<ArgumentException>(() => new TimeSlot(start));
    }
}
