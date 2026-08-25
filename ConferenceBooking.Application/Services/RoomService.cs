using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Exceptions;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Реалізація бізнес-сервісу управління конференц-залами.
/// </summary>
public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    /// <summary>
    /// Конструктор сервісу залів із впровадженням залежності репозиторію.
    /// </summary>
    /// <param name="roomRepository">Репозиторій доступу до даних залів.</param>
    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Створює новий конференц-зал та закріплює за ним перелік доступних послуг.
    /// </summary>
    public async Task<RoomDto> AddRoomAsync(CreateRoomDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            BasePricePerHour = dto.BasePricePerHour
        };

        AttachServices(room, dto.Services);

        await _roomRepository.AddAsync(room, cancellationToken);
        return MapToDto(room);
    }

    /// <summary>
    /// Отримує інформацію про зал за його ID.
    /// </summary>
    public async Task<RoomDto> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await GetRoomOrThrowAsync(id, cancellationToken);
        return MapToDto(room);
    }

    /// <summary>
    /// Отримує повний перелік усіх зареєстрованих залів.
    /// </summary>
    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(CancellationToken cancellationToken = default)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);
        return rooms.Select(MapToDto);
    }

    /// <summary>
    /// Оновлює параметри існуючого залу. Якщо передано новий список послуг — перезаписує попередній зв'язок.
    /// </summary>
    public async Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var room = await GetRoomOrThrowAsync(id, cancellationToken);

        room.UpdateDetails(dto.Name, dto.Capacity, dto.BasePricePerHour);

        if (dto.Services is not null)
        {
            room.ClearServices();
            AttachServices(room, dto.Services);
        }

        await _roomRepository.UpdateAsync(room, cancellationToken);
        return MapToDto(room);
    }

    /// <summary>
    /// Видаляє зал за ID або викидає виняток NotFoundException, якщо зал не знайдено.
    /// </summary>
    public async Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var isDeleted = await _roomRepository.DeleteAsync(id, cancellationToken);
        if (!isDeleted)
        {
            throw new NotFoundException($"Room with ID '{id}' was not found.");
        }
    }

    /// <summary>
    /// Шукає зали з необхідною місткістю, які не мають часових перетинів з існуючими бронюваннями.
    /// </summary>
    public async Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(
        DateTime startTime,
        int durationMinutes,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        var endTime = startTime.AddMinutes(durationMinutes);
        var rooms = await _roomRepository.GetAvailableRoomsAsync(startTime, endTime, capacity, cancellationToken);
        return rooms.Select(MapToDto);
    }

    private async Task<Room> GetRoomOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(id, cancellationToken);
        if (room is null)
        {
            throw new NotFoundException($"Room with ID '{id}' was not found.");
        }

        return room;
    }

    private static void AttachServices(Room room, IEnumerable<CreateServiceDto> services)
    {
        foreach (var serviceDto in services)
        {
            var service = new Service
            {
                Name = serviceDto.Name,
                Price = serviceDto.Price
            };

            room.AddService(service);
        }
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            BasePricePerHour = room.BasePricePerHour,
            Services = room.RoomServices
                .Select(rs => new ServiceDto
                {
                    Id = rs.Service?.Id ?? rs.ServiceId,
                    Name = rs.Service?.Name ?? string.Empty,
                    Price = rs.Service?.Price ?? 0m
                })
                .ToList()
        };
    }
}
