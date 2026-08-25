using ConferenceBooking.Application.DTOs;

namespace ConferenceBooking.Application.Interfaces;

/// <summary>
/// Інтерфейс сервісу формування аналітичних звітів.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Формує звіт щодо виручки, кількості бронювань та середнього чека по кожному залу.
    /// </summary>
    Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Формує звіт щодо рівня завантаженості залів (сумарна кількість заброньованих годин).
    /// </summary>
    Task<IEnumerable<RoomUtilizationDto>> GetUtilizationReportAsync(ReportFilterDto? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Формує рейтинг популярності та прибутковості додаткових послуг.
    /// </summary>
    Task<IEnumerable<ServicePopularityDto>> GetServicePopularityReportAsync(ReportFilterDto? filter = null, CancellationToken cancellationToken = default);
}
