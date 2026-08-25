using ConferenceBooking.Domain.Services;

namespace ConferenceBooking.Tests;

public class PricingDomainServiceTests
{
    private readonly IPricingDomainService _service = new PricingDomainService();

    [Fact]
    public void CalculateTotalPrice_StandardHours_NoServices()
    {
        // 10:00–11:00 (1h Standard) = 2000 * 1.0 = 2000
        var startTime = new DateTime(2024, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 11, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(2000m, total);
    }

    [Fact]
    public void CalculateTotalPrice_MorningHours_Applies10PercentDiscount()
    {
        // 07:00–08:00 (1h Morning) = 2000 * 0.9 = 1800
        var startTime = new DateTime(2024, 9, 1, 7, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 8, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(1800m, total);
    }

    [Fact]
    public void CalculateTotalPrice_PeakHours_Applies15PercentSurcharge()
    {
        // 12:00–13:00 (1h Peak) = 2000 * 1.15 = 2300
        var startTime = new DateTime(2024, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 13, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(2300m, total);
    }

    [Fact]
    public void CalculateTotalPrice_EveningHours_Applies20PercentDiscount()
    {
        // 19:00–20:00 (1h Evening) = 2000 * 0.8 = 1600
        var startTime = new DateTime(2024, 9, 1, 19, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 20, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(1600m, total);
    }

    [Fact]
    public void CalculateTotalPrice_MorningToStandard_CalculatesSegmentsCorrectly()
    {
        // 08:00–10:00:
        // 08:00–09:00 (1h Morning):  2000 * 0.9 = 1800
        // 09:00–10:00 (1h Standard): 2000 * 1.0 = 2000
        // Total = 3800
        var startTime = new DateTime(2024, 9, 1, 8, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 10, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(3800m, total);
    }

    [Fact]
    public void CalculateTotalPrice_WithServices_AddsFixedServicePrices()
    {
        // 10:00–11:00 (1h Standard) = 2000 + 500 + 300 = 2800
        var startTime = new DateTime(2024, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 11, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, [500m, 300m]);

        Assert.Equal(2800m, total);
    }

    [Fact]
    public void CalculateTotalPrice_CrossesMultipleZones_CalculatesAccurately()
    {
        // 10:00–16:00 (6 hours):
        // 10:00–12:00 (2h Standard) = 2000 * 2 = 4000
        // 12:00–14:00 (2h Peak)     = 2000 * 1.15 * 2 = 4600
        // 14:00–16:00 (2h Standard) = 2000 * 2 = 4000
        // Total = 12600
        var startTime = new DateTime(2024, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 16, 0, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(12600m, total);
    }

    [Fact]
    public void CalculateTotalPrice_FractionalHours_CalculatesAccurately()
    {
        // 10:00–10:30 (0.5h Standard) = 2000 * 0.5 = 1000
        var startTime = new DateTime(2024, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 10, 30, 0);

        var total = _service.CalculateTotalPrice(2000m, startTime, endTime, []);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void CalculateTotalPrice_EndTimeBeforeStartTime_ThrowsArgumentException()
    {
        var startTime = new DateTime(2024, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2024, 9, 1, 10, 0, 0);

        Assert.Throws<ArgumentException>(() =>
            _service.CalculateTotalPrice(2000m, startTime, endTime, []));
    }

    [Fact]
    public void CalculateTotalPrice_EqualTimes_ThrowsArgumentException()
    {
        var time = new DateTime(2024, 9, 1, 10, 0, 0);

        Assert.Throws<ArgumentException>(() =>
            _service.CalculateTotalPrice(2000m, time, time, []));
    }
}
