using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для доступу до даних бронювань.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Отримує бронювання за ID разом із обраними послугами.
    /// </summary>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує список бронювань конкретного залу.
    /// </summary>
    Task<IEnumerable<Booking>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує всі бронювання в системі.
    /// </summary>
    Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Додає нове бронювання до бази даних.
    /// </summary>
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує бронювання, відфільтровані за діапазоном дат (From, To).
    /// </summary>
    Task<IEnumerable<Booking>> GetFilteredAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
