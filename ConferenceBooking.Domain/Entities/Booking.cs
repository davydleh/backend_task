namespace ConferenceBooking.Domain.Entities;

/// <summary>
/// Доменна сутність замовлення/бронювання конференц-залу.
/// </summary>
public class Booking
{
    /// <summary>
    /// Унікальний ідентифікатор бронювання (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Ідентифікатор заброньованого залу.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Навігаційна властивість на заброньований зал.
    /// </summary>
    public Room Room { get; set; } = null!;

    /// <summary>
    /// Дата та час початку оренди.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Дата та час завершення оренди.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Підсумкова вартість бронювання (залишок розрахунку залу з коефіцієнтами доби + послуги).
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Колекція замовлених додаткових послуг із зафіксованими цінами.
    /// </summary>
    public ICollection<BookingSelectedService> SelectedServices { get; set; } = [];

    /// <summary>
    /// Загальна тривалість оренди.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Додає вибрану додаткову послугу із зафіксованою ціною.
    /// </summary>
    public void AddSelectedService(Guid serviceId, decimal price, Service? service = null)
    {
        SelectedServices.Add(new BookingSelectedService
        {
            BookingId = Id,
            Booking = this,
            ServiceId = serviceId,
            Price = price,
            Service = service!
        });
    }
}

/// <summary>
/// Сутність обраної додаткової послуги у бронюванні (зберігає фіксовану вартість на момент бронювання).
/// </summary>
public class BookingSelectedService
{
    /// <summary>
    /// Унікальний ідентифікатор запису.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Ідентифікатор бронювання.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Навігаційна властивість на бронювання.
    /// </summary>
    public Booking Booking { get; set; } = null!;

    /// <summary>
    /// Ідентифікатор додаткової послуги.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Навігаційна властивість на сутність послуги.
    /// </summary>
    public Service Service { get; set; } = null!;

    /// <summary>
    /// Вартість послуги, зафіксована на момент створення бронювання.
    /// </summary>
    public decimal Price { get; set; }
}
