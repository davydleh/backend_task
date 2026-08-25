namespace ConferenceBooking.Domain.Entities;

/// <summary>
/// Доменна сутність додаткової послуги (наприклад, "Wi-Fi", "Проєктор", "Звукове обладнання", "Кейтеринг").
/// </summary>
public class Service
{
    /// <summary>
    /// Унікальний ідентифікатор послуги (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Назва додаткової послуги.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Фіксована базова вартість послуги за одне замовлення.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Зв'язки із залами, для яких доступна дана послуга.
    /// </summary>
    public ICollection<RoomServiceItem> RoomServices { get; set; } = [];
}
