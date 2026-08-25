using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Services;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Реалізація бізнес-сервісу управління бронюваннями конференц-залів.
/// </summary>
public class BookingService : IBookingService
{
    private const int MinimumRoomCapacity = 1;

    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IPricingDomainService _pricingDomainService;

    /// <summary>
    /// Конструктор сервісу бронювання із впровадженням залежностей репозиторіїв та доменного калькулятора цін.
    /// </summary>
    public BookingService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IPricingDomainService pricingDomainService)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _pricingDomainService = pricingDomainService;
    }

    /// <summary>
    /// Оформлює бронювання залу: перевіряє доступність, валідує послуги, фіксує історичну вартість послуг та розраховує підсумкову ціну.
    /// </summary>
    /// <param name="dto">Параметри бронювання (зал, час, тривалість, послуги).</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>DTO створеного бронювання.</returns>
    public async Task<BookingDto> BookRoomAsync(CreateBookingDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var room = await GetRoomOrThrowAsync(dto.RoomId, cancellationToken);
        var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

        // 1. Перевіряємо відсутність часових колізій із наявними бронюваннями
        await EnsureRoomIsAvailableAsync(room.Id, dto.StartTime, endTime, cancellationToken);

        // 2. Перевіряємо, чи всі обрані послуги дійсно доступні у конфігурації цього залу
        ValidateServicesAreAvailableForRoom(room, dto.ServiceIds);

        // 3. Розраховуємо динамічну вартість за погодинними тарифними зонами + суму послуг
        var servicePrices = dto.ServiceIds.Select(room.GetServicePrice).ToList();
        var totalPrice = _pricingDomainService.CalculateTotalPrice(
            room.BasePricePerHour,
            dto.StartTime,
            endTime,
            servicePrices);

        // 4. Створюємо сутність бронювання зі збереженням зліпка цін послуг
        var booking = CreateBooking(room, dto.StartTime, endTime, totalPrice, dto.ServiceIds);
        await _bookingRepository.AddAsync(booking, cancellationToken);

        return MapToDto(booking);
    }

    private async Task<Room> GetRoomOrThrowAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room is null)
        {
            throw new NotFoundException($"Room with ID '{roomId}' was not found.");
        }

        return room;
    }

    /// <summary>
    /// Переконується у відсутності часових перетинів з іншими замовленнями для вибраного залу.
    /// </summary>
    private async Task EnsureRoomIsAvailableAsync(
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        var availableRooms = await _roomRepository.GetAvailableRoomsAsync(
            startTime,
            endTime,
            MinimumRoomCapacity,
            cancellationToken);

        var isRoomAvailable = availableRooms.Any(r => r.Id == roomId);
        if (!isRoomAvailable)
        {
            throw new RoomUnavailableException($"Room with ID '{roomId}' is not available for the requested time period.");
        }
    }

    /// <summary>
    /// Валідує, чи всі замовлені послуги прив'язані до конференц-залу.
    /// </summary>
    private static void ValidateServicesAreAvailableForRoom(Room room, IEnumerable<Guid> requestedServiceIds)
    {
        var invalidServiceIds = requestedServiceIds
            .Where(serviceId => !room.HasService(serviceId))
            .ToList();

        if (invalidServiceIds.Count > 0)
        {
            var invalidIdsFormatted = string.Join(", ", invalidServiceIds);
            throw new DomainValidationException($"The following services are not available for this room: {invalidIdsFormatted}");
        }
    }

    /// <summary>
    /// Фіксує поточну вартість послуг у зв'язку бронювання, щоб майбутня зміна цін не впливала на історію та аналітику.
    /// </summary>
    private static Booking CreateBooking(
        Room room,
        DateTime startTime,
        DateTime endTime,
        decimal totalPrice,
        IEnumerable<Guid> serviceIds)
    {
        var booking = new Booking
        {
            RoomId = room.Id,
            Room = room,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = totalPrice
        };

        foreach (var serviceId in serviceIds)
        {
            var price = room.GetServicePrice(serviceId);
            booking.AddSelectedService(serviceId, price);
        }

        return booking;
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalPrice = booking.TotalPrice,
            ServiceIds = booking.SelectedServices.Select(s => s.ServiceId).ToList()
        };
    }
}
