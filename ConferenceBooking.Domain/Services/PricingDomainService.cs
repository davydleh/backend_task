namespace ConferenceBooking.Domain.Services;

/// <summary>
/// Реалізація доменного сервісу динамічного ціноутворення.
/// Забезпечує погодинне нарізання інтервалу бронювання та застосування коефіцієнтів доби.
/// </summary>
public class PricingDomainService : IPricingDomainService
{
    private const decimal MinutesPerHour = 60m;
    private const int CurrencyDecimalPlaces = 2;

    private const decimal StandardRate = 1.00m;
    private const decimal MorningDiscountRate = 0.90m;
    private const decimal PeakSurchargeRate = 1.15m;
    private const decimal EveningDiscountRate = 0.80m;

    /// <summary>
    /// Погодинні тарифні зони доби:
    /// - 06:00 – 09:00: Ранкова знижка 10% (коефіцієнт 0.90)
    /// - 09:00 – 12:00: Стандартний тариф (коефіцієнт 1.00)
    /// - 12:00 – 14:00: Піковий час +15% (коефіцієнт 1.15)
    /// - 14:00 – 18:00: Стандартний тариф (коефіцієнт 1.00)
    /// - 18:00 – 23:00: Вечірня знижка 20% (коефіцієнт 0.80)
    /// - 23:00 – 06:00: Стандартний тариф (коефіцієнт 1.00)
    /// </summary>
    private static readonly (TimeSpan Start, TimeSpan End, decimal Modifier)[] DailyZones =
    [
        (TimeSpan.FromHours(0), TimeSpan.FromHours(6), StandardRate),
        (TimeSpan.FromHours(6), TimeSpan.FromHours(9), MorningDiscountRate),
        (TimeSpan.FromHours(9), TimeSpan.FromHours(12), StandardRate),
        (TimeSpan.FromHours(12), TimeSpan.FromHours(14), PeakSurchargeRate),
        (TimeSpan.FromHours(14), TimeSpan.FromHours(18), StandardRate),
        (TimeSpan.FromHours(18), TimeSpan.FromHours(23), EveningDiscountRate),
        (TimeSpan.FromHours(23), TimeSpan.FromHours(24), StandardRate)
    ];

    /// <summary>
    /// Розраховує підсумкову вартість оренди конференц-залу з урахуванням тривалості, тарифних коефіцієнтів та обраних послуг.
    /// </summary>
    public decimal CalculateTotalPrice(
        decimal basePricePerHour,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<decimal> servicePrices)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("Час завершення оренди повинен бути суворо пізніше за час початку.");
        }

        var totalRoomPrice = CalculateTotalRoomPrice(basePricePerHour, startTime, endTime);
        var totalServicesPrice = servicePrices.Sum();

        return Math.Round(totalRoomPrice + totalServicesPrice, CurrencyDecimalPlaces);
    }

    /// <summary>
    /// Розбиває часовий інтервал по межах діб (північ) для коректного обрахунку багатоденних замовлень.
    /// </summary>
    private static decimal CalculateTotalRoomPrice(decimal basePricePerHour, DateTime startTime, DateTime endTime)
    {
        decimal totalRoomPrice = 0m;
        var current = startTime;

        while (current < endTime)
        {
            var dayEnd = current.Date.AddDays(1);
            var effectiveEnd = dayEnd > endTime ? endTime : dayEnd;

            totalRoomPrice += CalculateDayPrice(basePricePerHour, current, effectiveEnd);
            current = effectiveEnd;
        }

        return totalRoomPrice;
    }

    /// <summary>
    /// Нарізає інтервал доби на тарифні сегменти та обчислює вартість за фактично використані години/хвилини.
    /// </summary>
    private static decimal CalculateDayPrice(decimal basePricePerHour, DateTime start, DateTime end)
    {
        decimal total = 0m;
        var current = start;

        while (current < end)
        {
            var timeOfDay = current.TimeOfDay;
            var zone = DailyZones.First(z => timeOfDay >= z.Start && timeOfDay < z.End);

            var zoneEnd = current.Date + zone.End;
            var segmentEnd = zoneEnd > end ? end : zoneEnd;

            var hours = (decimal)(segmentEnd - current).TotalMinutes / MinutesPerHour;
            total += basePricePerHour * hours * zone.Modifier;

            current = segmentEnd;
        }

        return total;
    }
}
