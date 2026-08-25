namespace ConferenceBooking.Application.DTOs;

/// <summary>
/// Звітні фінансові дані по конференц-залу (виручка та середній чек).
/// </summary>
public class RevenueReportDto
{
    /// <summary>
    /// Унікальний ідентифікатор конференц-залу.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Назва конференц-залу.
    /// </summary>
    public string RoomName { get; set; } = string.Empty;

    /// <summary>
    /// Загальна виручка по залу за вибраний період.
    /// </summary>
    /// <example>15400.00</example>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Кількість оформлених бронювань залу за період.
    /// </summary>
    /// <example>5</example>
    public int TotalBookings { get; set; }

    /// <summary>
    /// Середній чек за одне бронювання.
    /// </summary>
    /// <example>3080.00</example>
    public decimal AverageBookingPrice { get; set; }
}

/// <summary>
/// Звітні дані щодо завантаженості та утилізації залу.
/// </summary>
public class RoomUtilizationDto
{
    /// <summary>
    /// Унікальний ідентифікатор конференц-залу.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Назва конференц-залу.
    /// </summary>
    public string RoomName { get; set; } = string.Empty;

    /// <summary>
    /// Сумарна кількість заброньованих годин за вибраний період.
    /// </summary>
    /// <example>18.5</example>
    public double TotalBookedHours { get; set; }

    /// <summary>
    /// Загальна кількість бронювань за період.
    /// </summary>
    /// <example>4</example>
    public int TotalBookings { get; set; }
}

/// <summary>
/// Звітні дані щодо популярності додаткових послуг.
/// </summary>
public class ServicePopularityDto
{
    /// <summary>
    /// Унікальний ідентифікатор послуги.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Назва послуги (наприклад, "Wi-Fi", "Проєктор", "Кейтеринг").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Скільки разів дану послугу замовляли в бронюваннях.
    /// </summary>
    /// <example>12</example>
    public int TimesBooked { get; set; }

    /// <summary>
    /// Загальний дохід, згенерований даною послугою за період.
    /// </summary>
    /// <example>6000.00</example>
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Фільтр за діапазоном дат для формування аналітичних звітів.
/// </summary>
public class ReportFilterDto
{
    /// <summary>
    /// Початкова дата та час вибірки (необов'язково).
    /// </summary>
    /// <example>2026-08-01T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>
    /// Кінцева дата та час вибірки (необов'язково).
    /// </summary>
    /// <example>2026-08-31T23:59:59Z</example>
    public DateTime? To { get; set; }
}
