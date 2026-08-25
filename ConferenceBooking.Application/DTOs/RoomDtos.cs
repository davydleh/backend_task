using System.ComponentModel.DataAnnotations;

namespace ConferenceBooking.Application.DTOs;

/// <summary>
/// Модель запиту для створення нового конференц-залу.
/// </summary>
public class CreateRoomDto
{
    /// <summary>
    /// Назва конференц-залу.
    /// </summary>
    /// <example>Зал А (Малий переговорний)</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Максимальна місткість залу (кількість осіб).
    /// </summary>
    /// <example>20</example>
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000.")]
    public int Capacity { get; set; }

    /// <summary>
    /// Базова вартість оренди за 1 годину (без урахування коефіцієнтів та додаткових послуг).
    /// </summary>
    /// <example>1500.00</example>
    [Range(0.01, double.MaxValue, ErrorMessage = "BasePricePerHour must be greater than 0.")]
    public decimal BasePricePerHour { get; set; }

    /// <summary>
    /// Перелік додаткових послуг, закріплених за даним залом.
    /// </summary>
    public List<CreateServiceDto> Services { get; set; } = [];
}

/// <summary>
/// Модель запиту для оновлення параметрів існуючого конференц-залу.
/// </summary>
public class UpdateRoomDto
{
    /// <summary>
    /// Нова назва конференц-залу (необов'язково).
    /// </summary>
    /// <example>Зал А (Оновлений)</example>
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; set; }

    /// <summary>
    /// Нова місткість залу (необов'язково).
    /// </summary>
    /// <example>25</example>
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000.")]
    public int? Capacity { get; set; }

    /// <summary>
    /// Нова базова вартість оренди за годину (необов'язково).
    /// </summary>
    /// <example>1800.00</example>
    [Range(0.01, double.MaxValue, ErrorMessage = "BasePricePerHour must be greater than 0.")]
    public decimal? BasePricePerHour { get; set; }

    /// <summary>
    /// Новий перелік закріплених послуг (якщо передано — повністю перезаписує попередній).
    /// </summary>
    public List<CreateServiceDto>? Services { get; set; }
}

/// <summary>
/// Модель конференц-залу з переліком доступних послуг.
/// </summary>
public class RoomDto
{
    /// <summary>
    /// Унікальний ідентифікатор залу (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Назва конференц-залу.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Місткість залу (кількість осіб).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Базова погодинна вартість оренди.
    /// </summary>
    public decimal BasePricePerHour { get; set; }

    /// <summary>
    /// Список доступних послуг для даного залу.
    /// </summary>
    public List<ServiceDto> Services { get; set; } = [];
}
