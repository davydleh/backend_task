using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Управління конференц-залами: створення, перегляд, оновлення, видалення та пошук доступних залів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера залів.
    /// </summary>
    /// <param name="roomService">Сервіс управління конференц-залами.</param>
    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Отримує повний список усіх конференц-залів із переліком закріплених за ними додаткових послуг.
    /// </summary>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Список усіх конференц-залів.</returns>
    /// <response code="200">Список залів успішно отримано.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRooms(CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetAllRoomsAsync(cancellationToken);
        return Ok(rooms);
    }

    /// <summary>
    /// Отримує детальні дані конкретного конференц-залу за його унікальним ідентифікатором (ID).
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор залу (GUID).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Дані обраного залу.</returns>
    /// <response code="200">Зал знайдено.</response>
    /// <response code="404">Зал із вказаним ID не знайдено.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetRoomByIdAsync(id, cancellationToken);
        return Ok(room);
    }

    /// <summary>
    /// Створює новий конференц-зал із можливістю відразу прив'язати доступні послуги (Wi-Fi, проєктор тощо).
    /// </summary>
    /// <param name="dto">Параметри створення залу (назва, місткість, базова погодинна ставка, список послуг).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Створений зал з присвоєним ID.</returns>
    /// <response code="201">Зал успішно створено.</response>
    /// <response code="400">Передано некоректні дані (наприклад, місткість &lt;= 0 або порожня назва).</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddRoom([FromBody] CreateRoomDto dto, CancellationToken cancellationToken)
    {
        var room = await _roomService.AddRoomAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, room);
    }

    /// <summary>
    /// Оновлює інформацію про існуючий зал (назву, місткість, тариф) та оновлює список послуг.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор залу.</param>
    /// <param name="dto">Нові параметри залу.</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Оновлені дані залу.</returns>
    /// <response code="200">Зал успішно оновлено.</response>
    /// <response code="400">Некоректні дані запиту.</response>
    /// <response code="404">Зал із вказаним ID не знайдено.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomDto dto, CancellationToken cancellationToken)
    {
        var updatedRoom = await _roomService.UpdateRoomAsync(id, dto, cancellationToken);
        return Ok(updatedRoom);
    }

    /// <summary>
    /// Видаляє конференц-зал за його ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор залу.</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Порожня відповідь зі статусом 204.</returns>
    /// <response code="204">Зал успішно видалено.</response>
    /// <response code="404">Зал із вказаним ID не знайдено.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteRoomAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Здійснює пошук вільних залів за часовим проміжком та мінімальною місткістю з перевіркою відсутності перетинів з іншими бронюваннями.
    /// </summary>
    /// <param name="startTime">Дата та час початку оренди (UTC/ISO-8601).</param>
    /// <param name="durationMinutes">Тривалість оренди у хвилинах.</param>
    /// <param name="capacity">Мінімальна необхідна кількість місць.</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Список доступних для бронювання залів.</returns>
    /// <response code="200">Пошук успішно виконано, повертається список доступних залів.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAvailableRooms(
        [FromQuery] DateTime startTime,
        [FromQuery] int durationMinutes,
        [FromQuery] int capacity,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomService.SearchAvailableRoomsAsync(startTime, durationMinutes, capacity, cancellationToken);
        return Ok(rooms);
    }
}
