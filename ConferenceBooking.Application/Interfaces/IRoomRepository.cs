using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для доступу до даних конференц-залів.
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Отримує зал за ID разом із закріпленими послугами.
    /// </summary>
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує всі зали разом із закріпленими послугами.
    /// </summary>
    Task<IEnumerable<Room>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує зали з необхідною місткістю, вільні в заданий проміжок часу.
    /// </summary>
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int capacity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Зберігає новий зал у сховищі даних.
    /// </summary>
    Task AddAsync(Room room, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновлює дані залу у сховищі.
    /// </summary>
    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видаляє зал за його унікальним ідентифікатором.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
