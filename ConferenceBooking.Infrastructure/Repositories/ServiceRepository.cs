using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи з довідником додаткових послуг.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Ініціалізує новий екземпляр репозиторію послуг.
    /// </summary>
    /// <param name="context">Контекст бази даних EF Core.</param>
    public ServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримує список послуг за вказаним переліком унікальних ідентифікаторів.
    /// </summary>
    public async Task<IEnumerable<Service>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Services
            .AsNoTracking()
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Отримує всі доступні в довіднику послуги.
    /// </summary>
    public async Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
