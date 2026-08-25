namespace ConferenceBooking.Domain.Services;

/// <summary>
/// Інтерфейс доменного сервісу розрахунку динамічної вартості оренди конференц-залів.
/// </summary>
public interface IPricingDomainService
{
    /// <summary>
    /// Розраховує підсумкову вартість оренди з урахуванням погодинних часових зон доби та фіксованих послуг.
    /// </summary>
    /// <param name="basePricePerHour">Базова погодинна ставка залу.</param>
    /// <param name="startTime">Дата та час початку бронювання.</param>
    /// <param name="endTime">Дата та час завершення бронювання.</param>
    /// <param name="servicePrices">Список цін обраних додаткових послуг.</param>
    /// <returns>Підсумкова вартість оренди, округлена до 2 знаків після коми.</returns>
    decimal CalculateTotalPrice(
        decimal basePricePerHour,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<decimal> servicePrices);
}
