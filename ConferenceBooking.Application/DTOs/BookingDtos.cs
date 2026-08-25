using System.ComponentModel.DataAnnotations;

namespace ConferenceBooking.Application.DTOs;

/// <summary>
/// Модель запиту для оформлення бронювання конференц-залу.
/// </summary>
public class CreateBookingDto
{
    /// <summary>
    /// Унікальний ідентифікатор залу, який бронюється.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "Room ID is required.")]
    public Guid RoomId { get; set; }

    /// <summary>
    /// Дата та час початку оренди (UTC або локальний час у форматі ISO-8601).
    /// </summary>
    /// <example>2026-08-25T10:00:00Z</example>
    [Required(ErrorMessage = "Start time is required.")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Тривалість оренди у хвилинах (від 1 до 1440 хвилин).
    /// </summary>
    /// <example>120</example>
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Список ідентифікаторів додаткових послуг (Wi-Fi, кейтеринг, проєктор тощо), які додаються до бронювання.
    /// </summary>
    public List<Guid> ServiceIds { get; set; } = [];
}

/// <summary>
/// Модель створеного бронювання з підсумковою розрахованою вартістю.
/// </summary>
public class BookingDto
{
    /// <summary>
    /// Унікальний ідентифікатор створеного бронювання (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Ідентифікатор заброньованого залу.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Дата та час початку бронювання.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Дата та час завершення бронювання.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Підсумкова вартість оренди залу (з урахуванням погодинних коефіцієнтів та фіксованих цін обраних послуг).
    /// </summary>
    /// <example>3500.00</example>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Список ідентифікаторів обраних послуг.
    /// </summary>
    public List<Guid> ServiceIds { get; set; } = [];
}
