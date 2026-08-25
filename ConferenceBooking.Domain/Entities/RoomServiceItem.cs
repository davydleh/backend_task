namespace ConferenceBooking.Domain.Entities;

/// <summary>
/// Проміжна сутність зв'язку many-to-many між конференц-залом та доступною в ньому послугою.
/// </summary>
public class RoomServiceItem
{
    /// <summary>
    /// Ідентифікатор залу.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Навігаційна властивість на зал.
    /// </summary>
    public Room Room { get; set; } = null!;

    /// <summary>
    /// Ідентифікатор послуги.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Навігаційна властивість на послугу.
    /// </summary>
    public Service Service { get; set; } = null!;
}
