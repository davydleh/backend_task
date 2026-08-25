using System.ComponentModel.DataAnnotations;

namespace ConferenceBooking.Application.DTOs;

/// <summary>
/// Модель для створення або прив'язки додаткової послуги до залу.
/// </summary>
public class CreateServiceDto
{
    /// <summary>
    /// Назва додаткової послуги (наприклад, "Wi-Fi", "Проєктор", "Звукове обладнання").
    /// </summary>
    /// <example>Проєктор 4K</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Фіксована вартість послуги за одне бронювання.
    /// </summary>
    /// <example>500.00</example>
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }
}

/// <summary>
/// Модель додаткової послуги.
/// </summary>
public class ServiceDto
{
    /// <summary>
    /// Унікальний ідентифікатор послуги (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Назва послуги.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Фіксована вартість послуги за одне замовлення.
    /// </summary>
    public decimal Price { get; set; }
}
