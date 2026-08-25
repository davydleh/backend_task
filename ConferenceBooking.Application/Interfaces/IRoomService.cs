using ConferenceBooking.Application.DTOs;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс сервісу управління конференц-залами.
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Створює новий зал із переліком додаткових послуг.
    /// </summary>
    Task<RoomDto> AddRoomAsync(CreateRoomDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Повертає зал за його унікальним ідентифікатором.
    /// </summary>
    Task<RoomDto> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Повертає повний список усіх конференц-залів.
    /// </summary>
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновлює дані залу (назву, місткість, тариф) та його послуги.
    /// </summary>
    Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видаляє конференц-зал за ідентифікатором.
    /// </summary>
    Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Здійснює пошук вільних залів із достатньою місткістю без часових колізій.
    /// </summary>
    Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(DateTime startTime, int durationMinutes, int capacity, CancellationToken cancellationToken = default);
}
