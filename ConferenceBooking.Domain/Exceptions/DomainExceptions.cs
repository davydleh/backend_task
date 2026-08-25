namespace ConferenceBooking.Domain.Exceptions;

/// <summary>
/// Базовий абстрактний клас для всіх винятків доменного рівня.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Конструктор базового доменного винятку.
    /// </summary>
    /// <param name="message">Повідомлення про помилку.</param>
    protected DomainException(string message) : base(message) { }
}

/// <summary>
/// Виняток, що виникає, коли запитуваний ресурс (зал, бронювання тощо) не знайдено.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="NotFoundException"/>.
    /// </summary>
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Виняток, що сигналізує про зайнятість залу в запитуваний часовий інтервал.
/// </summary>
public class RoomUnavailableException : DomainException
{
    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="RoomUnavailableException"/>.
    /// </summary>
    public RoomUnavailableException(string message) : base(message) { }
}

/// <summary>
/// Виняток, що сигналізує про порушення бізнес-правил та обмежень доменної моделі.
/// </summary>
public class DomainValidationException : DomainException
{
    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="DomainValidationException"/>.
    /// </summary>
    public DomainValidationException(string message) : base(message) { }
}
