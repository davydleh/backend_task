using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Exceptions;

namespace ConferenceBooking.Tests;

public class RoomServiceTests
{
    private readonly FakeRoomRepository _roomRepository = new();
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        _service = new RoomService(_roomRepository);
    }

    [Fact]
    public async Task AddRoomAsync_ValidDto_CreatesAndReturnsRoomDto()
    {
        var dto = new CreateRoomDto
        {
            Name = "Conference Room Alpha",
            Capacity = 25,
            BasePricePerHour = 1500m,
            Services =
            [
                new CreateServiceDto { Name = "Whiteboard", Price = 100m }
            ]
        };

        var result = await _service.AddRoomAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Conference Room Alpha", result.Name);
        Assert.Equal(25, result.Capacity);
        Assert.Equal(1500m, result.BasePricePerHour);
        Assert.Single(result.Services);
        Assert.Single(_roomRepository.Rooms);
    }

    [Fact]
    public async Task GetRoomByIdAsync_RoomExists_ReturnsRoomDto()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Existing Room", Capacity = 10, BasePricePerHour = 500m };
        _roomRepository.Rooms.Add(room);

        var result = await _service.GetRoomByIdAsync(room.Id);

        Assert.Equal(room.Id, result.Id);
        Assert.Equal("Existing Room", result.Name);
    }

    [Fact]
    public async Task GetRoomByIdAsync_RoomNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetRoomByIdAsync(nonExistentId));
        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task UpdateRoomAsync_RoomExists_UpdatesPropertiesAndReturnsDto()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Old Room", Capacity = 10, BasePricePerHour = 500m };
        _roomRepository.Rooms.Add(room);

        var updateDto = new UpdateRoomDto
        {
            Name = "Updated Room",
            Capacity = 20,
            BasePricePerHour = 750m
        };

        var result = await _service.UpdateRoomAsync(room.Id, updateDto);

        Assert.Equal("Updated Room", result.Name);
        Assert.Equal(20, result.Capacity);
        Assert.Equal(750m, result.BasePricePerHour);
    }

    [Fact]
    public async Task UpdateRoomAsync_RoomNotFound_ThrowsNotFoundException()
    {
        var updateDto = new UpdateRoomDto { Name = "Updated Room" };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateRoomAsync(Guid.NewGuid(), updateDto));
    }

    [Fact]
    public async Task DeleteRoomAsync_RoomExists_DeletesSuccessfully()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Room To Delete", Capacity = 10, BasePricePerHour = 500m };
        _roomRepository.Rooms.Add(room);

        await _service.DeleteRoomAsync(room.Id);

        Assert.Empty(_roomRepository.Rooms);
    }

    [Fact]
    public async Task DeleteRoomAsync_RoomNotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteRoomAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SearchAvailableRoomsAsync_ReturnsMatchingRooms()
    {
        var room = new Room { Id = Guid.NewGuid(), Name = "Available Room", Capacity = 50, BasePricePerHour = 1000m };
        _roomRepository.AvailableRooms.Add(room);

        var result = await _service.SearchAvailableRoomsAsync(DateTime.UtcNow, 60, 20);

        Assert.Single(result);
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
            Task.FromResult<IEnumerable<Room>>(AvailableRooms.Where(r => r.Capacity >= capacity));

        public Task AddAsync(Room room, CancellationToken cancellationToken = default)
        {
            Rooms.Add(room);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Room room, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Rooms.RemoveAll(r => r.Id == id) > 0);
    }
}
