using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Реалізація сервісу аналітичної звітності (виручка, завантаженість залів, рейтинг послуг).
/// </summary>
public class ReportService : IReportService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    /// <summary>
    /// Конструктор сервісу звітів із впровадженням репозиторіїв бронювань та залів.
    /// </summary>
    public ReportService(IBookingRepository bookingRepository, IRoomRepository roomRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Формує фінансовий звіт: сумарний дохід, кількість замовлень та середній чек по кожному залу за період.
    /// </summary>
    public async Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(
        ReportFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var activeFilter = filter ?? new ReportFilterDto();
        var (rooms, bookingsByRoom) = await LoadRoomsAndGroupedBookingsAsync(activeFilter, cancellationToken);

        return rooms.Select(room =>
        {
            var roomBookings = bookingsByRoom.GetValueOrDefault(room.Id, []);
            var totalRevenue = roomBookings.Sum(b => b.TotalPrice);
            var totalBookings = roomBookings.Count;
            var averagePrice = totalBookings > 0
                ? Math.Round(totalRevenue / totalBookings, 2)
                : 0m;

            return new RevenueReportDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                AverageBookingPrice = averagePrice
            };
        });
    }

    /// <summary>
    /// Формує звіт завантаженості залів: сумарна кількість заброньованих годин та замовлень по кожному залу.
    /// </summary>
    public async Task<IEnumerable<RoomUtilizationDto>> GetUtilizationReportAsync(
        ReportFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var activeFilter = filter ?? new ReportFilterDto();
        var (rooms, bookingsByRoom) = await LoadRoomsAndGroupedBookingsAsync(activeFilter, cancellationToken);

        return rooms.Select(room =>
        {
            var roomBookings = bookingsByRoom.GetValueOrDefault(room.Id, []);
            return new RoomUtilizationDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                TotalBookedHours = Math.Round(roomBookings.Sum(b => b.Duration.TotalHours), 2),
                TotalBookings = roomBookings.Count
            };
        });
    }

    /// <summary>
    /// Формує аналітику популярності та дохідності додаткових послуг із сортуванням за кількістю викликів.
    /// </summary>
    public async Task<IEnumerable<ServicePopularityDto>> GetServicePopularityReportAsync(
        ReportFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var activeFilter = filter ?? new ReportFilterDto();
        var bookings = await _bookingRepository.GetFilteredAsync(activeFilter.From, activeFilter.To, cancellationToken);

        return bookings
            .SelectMany(b => b.SelectedServices)
            .GroupBy(bs => new { bs.ServiceId, Name = bs.Service?.Name ?? "Невідома послуга" })
            .Select(g => new ServicePopularityDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                TimesBooked = g.Count(),
                TotalRevenue = g.Sum(bs => bs.Price)
            })
            .OrderByDescending(s => s.TimesBooked)
            .ToList();
    }

    /// <summary>
    /// Завантажує зали та групує бронювання за ID залу для уникнення проблеми N+1 запитів до БД.
    /// </summary>
    private async Task<(IEnumerable<Room> Rooms, Dictionary<Guid, List<Booking>> BookingsByRoom)> LoadRoomsAndGroupedBookingsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);
        var bookings = await _bookingRepository.GetFilteredAsync(filter.From, filter.To, cancellationToken);

        var bookingsByRoom = bookings
            .GroupBy(b => b.RoomId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return (rooms, bookingsByRoom);
    }
}
