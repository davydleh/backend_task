using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Services;

namespace ConferenceBooking.Tests;

public class BookingServiceTests
{
    private readonly FakeRoomRepository _roomRepository = new();
    private readonly FakeBookingRepository _bookingRepository = new();
    private readonly IPricingDomainService _pricingService = new PricingDomainService();
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _service = new BookingService(_bookingRepository, _roomRepository, _pricingService);
    }

    [Fact]
    public async Task BookRoomAsync_RoomNotFound_ThrowsNotFoundException()
    {
        var dto = new CreateBookingDto
        {
            RoomId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            DurationMinutes = 60
        };

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.BookRoomAsync(dto));
        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task BookRoomAsync_RoomUnavailable_ThrowsRoomUnavailableException()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Room 1", Capacity = 10, BasePricePerHour = 1000 };
        _roomRepository.Rooms.Add(room);

        // Simulate unavailable room
        _roomRepository.AvailableRooms.Clear();

        var dto = new CreateBookingDto
        {
            RoomId = room.Id,
            StartTime = DateTime.UtcNow,
            DurationMinutes = 60
        };

        var exception = await Assert.ThrowsAsync<RoomUnavailableException>(() => _service.BookRoomAsync(dto));
        Assert.Contains("is not available", exception.Message);
    }

    [Fact]
    public async Task BookRoomAsync_InvalidServices_ThrowsDomainValidationException()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Room 1", Capacity = 10, BasePricePerHour = 1000 };
        _roomRepository.Rooms.Add(room);
        _roomRepository.AvailableRooms.Add(room);

        var invalidServiceId = Guid.NewGuid();
        var dto = new CreateBookingDto
        {
            RoomId = room.Id,
            StartTime = new DateTime(2025, 1, 1, 10, 0, 0),
            DurationMinutes = 60,
            ServiceIds = [invalidServiceId]
        };

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() => _service.BookRoomAsync(dto));
        Assert.Contains(invalidServiceId.ToString(), exception.Message);
    }

    [Fact]
    public async Task BookRoomAsync_ValidRequest_CreatesBookingAndReturnsDto()
    {
        var service = new Service { Id = Guid.NewGuid(), Name = "Projector", Price = 500 };
        var room = new Room { Id = Guid.NewGuid(), Name = "Room 1", Capacity = 10, BasePricePerHour = 1000 };
        room.AddService(service);

        _roomRepository.Rooms.Add(room);
        _roomRepository.AvailableRooms.Add(room);

        var startTime = new DateTime(2025, 1, 1, 10, 0, 0); // 10:00 - Standard hour
        var dto = new CreateBookingDto
        {
            RoomId = room.Id,
            StartTime = startTime,
            DurationMinutes = 120, // 2 hours = 2000 + 500 = 2500
            ServiceIds = [service.Id]
        };

        var result = await _service.BookRoomAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(2500m, result.TotalPrice);
        Assert.Single(_bookingRepository.Bookings);
    }

    private class FakeRoomRepository : IRoomRepository
    {
        public List<Room> Rooms { get; } = [];
        public List<Room> AvailableRooms { get; } = [];

        public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Rooms.FirstOrDefault(r => r.Id == id));

        public Task<IEnumerable<Room>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Room>>(Rooms);

        public Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int capacity, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Room>>(AvailableRooms);

        public Task AddAsync(Room room, CancellationToken cancellationToken = default)
        {
            Rooms.Add(room);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Room room, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Rooms.RemoveAll(r => r.Id == id) > 0);
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

        public Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            Bookings.Add(booking);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Booking>> GetFilteredAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Booking>>(Bookings);
    }
}
