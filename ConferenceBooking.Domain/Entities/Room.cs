namespace ConferenceBooking.Domain.Entities;

/// <summary>
/// Доменна сутність конференц-залу.
/// </summary>
public class Room
{
    /// <summary>
    /// Унікальний ідентифікатор залу.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Назва залу (наприклад, "Зал А (Малий переговорний)").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Місткість залу (максимальна кількість учасників).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Базова погодинна ставка оренди (до застосування часових коефіцієнтів доби).
    /// </summary>
    public decimal BasePricePerHour { get; set; }

    /// <summary>
    /// Колекція зв'язків із закріпленими за залом послугами.
    /// </summary>
    public ICollection<RoomServiceItem> RoomServices { get; set; } = [];

    /// <summary>
    /// Додає доступну послугу до конфігурації даного залу.
    /// </summary>
    public void AddService(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);
        RoomServices.Add(new RoomServiceItem
        {
            RoomId = Id,
            Room = this,
            ServiceId = service.Id,
            Service = service
        });
    }

    /// <summary>
    /// Очищає список закріплених за залом послуг.
    /// </summary>
    public void ClearServices()
    {
        RoomServices.Clear();
    }

    /// <summary>
    /// Оновлює базові атрибути залу.
    /// </summary>
    public void UpdateDetails(string? name, int? capacity, decimal? basePricePerHour)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        if (capacity.HasValue && capacity.Value > 0)
        {
            Capacity = capacity.Value;
        }

        if (basePricePerHour.HasValue && basePricePerHour.Value > 0)
        {
            BasePricePerHour = basePricePerHour.Value;
        }
    }

    /// <summary>
    /// Перевіряє, чи закріплена конкретна послуга за цим залом.
    /// </summary>
    public bool HasService(Guid serviceId) =>
        RoomServices.Any(rs => rs.ServiceId == serviceId);

    /// <summary>
    /// Отримує актуальну ціну послуги, прив'язаної до залу.
    /// </summary>
    public decimal GetServicePrice(Guid serviceId) =>
        RoomServices.FirstOrDefault(rs => rs.ServiceId == serviceId)?.Service?.Price 
        ?? throw new InvalidOperationException($"Послугу з ID '{serviceId}' не закріплено за цим залом.");
}
