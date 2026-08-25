using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи з даними конференц-залів за допомогою Entity Framework Core.
/// </summary>
public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Ініціалізує новий екземпляр репозиторію залів.
    /// </summary>
    /// <param name="context">Контекст бази даних EF Core.</param>
    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримує зал за ID разом із закріпленими послугами.
    /// </summary>
    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .Include(r => r.RoomServices)
                .ThenInclude(rs => rs.Service)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримує всі конференц-зали з усіма пов'язаними послугами.
    /// </summary>
    public async Task<IEnumerable<Room>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.RoomServices)
                .ThenInclude(rs => rs.Service)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Повертає список залів, що задовольняють вимогу місткості та не мають перетинів за часом із наявними бронюваннями.
    /// </summary>
    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        DateTime startTime,
        DateTime endTime,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        // Перетин існує тоді й тільки тоді, коли:
        // початок існуючого < кінець нового І кінець існуючого > початок нового
        var bookedRoomIds = _context.Bookings
            .AsNoTracking()
            .Where(b => b.StartTime < endTime && b.EndTime > startTime)
            .Select(b => b.RoomId);

        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.RoomServices)
                .ThenInclude(rs => rs.Service)
            .Where(r => r.Capacity >= capacity && !bookedRoomIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Додає новий зал до бази даних.
    /// </summary>
    public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        await _context.Rooms.AddAsync(room, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Оновлює інформацію про зал.
    /// </summary>
    public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Видаляє зал за його ID за допомогою швидкого прямого видалення ExecuteDeleteAsync.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _context.Rooms
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }
}
