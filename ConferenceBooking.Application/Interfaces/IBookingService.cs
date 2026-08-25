using ConferenceBooking.Application.DTOs;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс сервісу управління бронюваннями конференц-залів.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Оформлює бронювання залу з динамічним розрахунком вартості.
    /// </summary>
    /// <param name="dto">Параметри бронювання.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Створене бронювання із підсумковою сумою.</returns>
    Task<BookingDto> BookRoomAsync(CreateBookingDto dto, CancellationToken cancellationToken = default);
}
