using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для доступу до довідника додаткових послуг.
/// </summary>
public interface IServiceRepository
{
    /// <summary>
    /// Отримує послуги за їхнім списком ідентифікаторів.
    /// </summary>
    Task<IEnumerable<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує повний список усіх зареєстрованих послуг.
    /// </summary>
    Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default);
}
