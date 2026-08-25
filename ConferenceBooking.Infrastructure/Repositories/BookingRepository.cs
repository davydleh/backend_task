using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи з даними бронювань за допомогою Entity Framework Core.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Ініціалізує новий екземпляр репозиторію бронювань.
    /// </summary>
    /// <param name="context">Контекст бази даних EF Core.</param>
    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримує бронювання за ID разом із обраними послугами.
    /// </summary>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.SelectedServices)
                .ThenInclude(bs => bs.Service)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримує список бронювань конкретного залу.
    /// </summary>
    public async Task<IEnumerable<Booking>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Отримує всі бронювання в системі.
    /// </summary>
    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.SelectedServices)
                .ThenInclude(bs => bs.Service)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Додає нове бронювання до бази даних та фіксує транзакцію.
    /// </summary>
    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await _context.Bookings.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Отримує вибірку бронювань із опціональною фільтрацією за часовим проміжком (From, To).
    /// </summary>
    public async Task<IEnumerable<Booking>> GetFilteredAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.SelectedServices)
                .ThenInclude(bs => bs.Service)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(b => b.StartTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.EndTime <= to.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
