using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Tests;

public class ReportServiceTests
{
    private readonly FakeRoomRepository _roomRepository = new();
    private readonly FakeBookingRepository _bookingRepository = new();
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _service = new ReportService(_bookingRepository, _roomRepository);
    }

    [Fact]
    public async Task GetRevenueReportAsync_CalculatesCorrectRevenueAndAverages()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Room Alpha", Capacity = 10, BasePricePerHour = 1000m };
        _roomRepository.Rooms.Add(room);

        _bookingRepository.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            StartTime = new DateTime(2025, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2025, 1, 1, 12, 0, 0),
            TotalPrice = 2000m
        });

        _bookingRepository.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            StartTime = new DateTime(2025, 1, 2, 10, 0, 0),
            EndTime = new DateTime(2025, 1, 2, 11, 0, 0),
            TotalPrice = 1000m
        });

        var report = (await _service.GetRevenueReportAsync()).ToList();

        Assert.Single(report);
        var item = report[0];
        Assert.Equal(room.Id, item.RoomId);
        Assert.Equal("Room Alpha", item.RoomName);
        Assert.Equal(3000m, item.TotalRevenue);
        Assert.Equal(2, item.TotalBookings);
        Assert.Equal(1500m, item.AverageBookingPrice);
    }

    [Fact]
    public async Task GetUtilizationReportAsync_CalculatesTotalBookedHours()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Room Beta", Capacity = 20, BasePricePerHour = 1500m };
        _roomRepository.Rooms.Add(room);

        _bookingRepository.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            StartTime = new DateTime(2025, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2025, 1, 1, 13, 30, 0), // 3.5 hours
            TotalPrice = 5250m
        });

        var report = (await _service.GetUtilizationReportAsync()).ToList();

        Assert.Single(report);
        var item = report[0];
        Assert.Equal(3.5, item.TotalBookedHours);
        Assert.Equal(1, item.TotalBookings);
    }

    [Fact]
    public async Task GetServicePopularityReportAsync_GroupsAndSortsByPopularity()
    {
        var projectorId = Guid.NewGuid();
        var wifiId = Guid.NewGuid();

        var projector = new Service { Id = projectorId, Name = "Projector", Price = 500m };
        var wifi = new Service { Id = wifiId, Name = "Wi-Fi", Price = 200m };

        var booking1 = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        booking1.AddSelectedService(projector.Id, projector.Price, projector);
        booking1.AddSelectedService(wifi.Id, wifi.Price, wifi);

        var booking2 = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1)
        };
        booking2.AddSelectedService(projector.Id, projector.Price, projector);

        _bookingRepository.Bookings.Add(booking1);
        _bookingRepository.Bookings.Add(booking2);

        var report = (await _service.GetServicePopularityReportAsync()).ToList();

        Assert.Equal(2, report.Count);
        Assert.Equal("Projector", report[0].ServiceName);
        Assert.Equal(2, report[0].TimesBooked);
        Assert.Equal(1000m, report[0].TotalRevenue);

        Assert.Equal("Wi-Fi", report[1].ServiceName);
        Assert.Equal(1, report[1].TimesBooked);
        Assert.Equal(200m, report[1].TotalRevenue);
    }

    private class FakeRoomRepository : IRoomRepository
    {
        public List<Room> Rooms { get; } = [];

        public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Rooms.FirstOrDefault(r => r.Id == id));

        public Task<IEnumerable<Room>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Room>>(Rooms);

        public Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int capacity, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Room>>(Rooms);

        public Task AddAsync(Room room, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Room room, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private class FakeBookingRepository : IBookingRepository
    {
        public List<Booking> Bookings { get; } = [];

        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Bookings.FirstOrDefault(b => b.Id == id));

        public Task<IEnumerable<Booking>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Booking>>(Bookings.Where(b => b.RoomId == roomId));

        public Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Booking>>(Bookings);

        public Task AddAsync(Booking booking, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IEnumerable<Booking>> GetFilteredAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Booking>>(Bookings);
    }
}
