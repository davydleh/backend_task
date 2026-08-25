using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Управління процесом бронювання конференц-залів та динамічного розрахунку вартості оренди.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера бронювань.
    /// </summary>
    /// <param name="bookingService">Сервіс управління бронюваннями.</param>
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Оформлює нове бронювання конференц-залу з автоматичним динамічним розрахунком вартості за часовими зонами доби та додатковими послугами.
    /// </summary>
    /// <remarks>
    /// **Правила ціноутворення:**
    /// - **06:00 – 09:00**: Ранкова знижка 10% (коефіцієнт 0.90)
    /// - **09:00 – 12:00**: Стандартний тариф (коефіцієнт 1.00)
    /// - **12:00 – 14:00**: Пікові години +15% (коефіцієнт 1.15)
    /// - **14:00 – 18:00**: Стандартний тариф (коефіцієнт 1.00)
    /// - **18:00 – 23:00**: Вечірня знижка 20% (коефіцієнт 0.80)
    /// - **23:00 – 06:00**: Стандартний тариф (коефіцієнт 1.00)
    /// 
    /// До вартості оренди залу додається фіксована вартість обраних додаткових послуг (Wi-Fi, проєктор тощо), зафіксована на момент створення заявки.
    /// </remarks>
    /// <param name="dto">Параметри бронювання (ідентифікатор залу, дата/час початку, тривалість у хвилинах, перелік ID обраних послуг).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Створене бронювання із загальною розрахованою сумою до сплати.</returns>
    /// <response code="201">Бронювання успішно оформлено та розраховано.</response>
    /// <response code="400">Помилка валідації (зал зайнятий на цей час або обрано послуги, недоступні для цього залу).</response>
    /// <response code="404">Вказаний конференц-зал не знайдено.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BookRoom([FromBody] CreateBookingDto dto, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.BookRoomAsync(dto, cancellationToken);
        return Created(string.Empty, booking);
    }
}
