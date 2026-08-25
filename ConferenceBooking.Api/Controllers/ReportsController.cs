using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Надає аналітичну звітність щодо виручки, завантаженості залів та популярності додаткових послуг.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера звітів.
    /// </summary>
    /// <param name="reportService">Сервіс аналітики та формування звітів.</param>
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Отримує звіт щодо загальної виручки, кількості бронювань та середнього чека по кожному залу за вказаний період.
    /// </summary>
    /// <param name="filter">Параметри фільтрації за діапазоном дат (From, To).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Список фінансових показників по кожному залу.</returns>
    /// <response code="200">Звіт успішно сформовано.</response>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(IEnumerable<RevenueReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueReport([FromQuery] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetRevenueReportAsync(filter, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Отримує звіт щодо завантаженості залів (сумарна кількість заброньованих годин та кількість бронювань).
    /// </summary>
    /// <param name="filter">Параметри фільтрації за діапазоном дат (From, To).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Список показників утилізації по кожному залу.</returns>
    /// <response code="200">Звіт успішно сформовано.</response>
    [HttpGet("utilization")]
    [ProducesResponseType(typeof(IEnumerable<RoomUtilizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUtilizationReport([FromQuery] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetUtilizationReportAsync(filter, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Отримує звіт популярності та прибутковості додаткових послуг (Wi-Fi, кейтеринг, проєктор тощо).
    /// </summary>
    /// <param name="filter">Параметри фільтрації за діапазоном дат (From, To).</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Рейтинг послуг, відсортований за спаданням кількості замовлень.</returns>
    /// <response code="200">Звіт успішно сформовано.</response>
    [HttpGet("services")]
    [ProducesResponseType(typeof(IEnumerable<ServicePopularityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServicePopularityReport([FromQuery] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetServicePopularityReportAsync(filter, cancellationToken);
        return Ok(report);
    }
}
