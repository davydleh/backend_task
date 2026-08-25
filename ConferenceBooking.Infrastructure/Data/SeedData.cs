using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceBooking.Infrastructure.Data;

/// <summary>
/// Клас для автоматичної ініціалізації структури бази даних та наповнення початковими тестовими даними.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Перевіряє існування БД та наповнює її початковими залами, послугами та історією бронювань, якщо база порожня.
    /// </summary>
    /// <param name="serviceProvider">Провайдер сервісів додатку.</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingService = scope.ServiceProvider.GetRequiredService<IPricingDomainService>();

        context.Database.EnsureCreated();

        // Якщо в базі вже є зали, пропускаємо наповнення (ідемпотентність)
        if (context.Rooms.Any())
        {
            return;
        }

        // 1. Створюємо базовий перелік додаткових послуг
        var projector = new Service { Name = "Проєктор", Price = 500 };
        var wifi = new Service { Name = "Wi-Fi", Price = 300 };
        var sound = new Service { Name = "Звукове обладнання", Price = 700 };
        var catering = new Service { Name = "Кейтеринг", Price = 1200 };
        var video = new Service { Name = "Відеоконференція", Price = 800 };

        context.Services.AddRange(projector, wifi, sound, catering, video);

        // 2. Створюємо конференц-зали з різною місткістю та базовою ціною
        var roomA = new Room { Name = "Зал А (Малий переговорний)", Capacity = 15, BasePricePerHour = 1000 };
        var roomB = new Room { Name = "Зал B (Конференц-зал)", Capacity = 50, BasePricePerHour = 2000 };
        var roomC = new Room { Name = "Зал C (Гранд-хол)", Capacity = 150, BasePricePerHour = 4500 };
        var roomD = new Room { Name = "Зал D (VIP-лаунж)", Capacity = 30, BasePricePerHour = 3000 };

        context.Rooms.AddRange(roomA, roomB, roomC, roomD);

        // Допоміжна функція прив'язки послуг до залу
        void LinkServices(Room room, params Service[] services)
        {
            foreach (var s in services)
            {
                room.RoomServices.Add(new RoomServiceItem
                {
                    Room = room,
                    RoomId = room.Id,
                    Service = s,
                    ServiceId = s.Id
                });
            }
        }

        LinkServices(roomA, wifi, projector);
        LinkServices(roomB, wifi, projector, sound, video);
        LinkServices(roomC, wifi, projector, sound, catering, video);
        LinkServices(roomD, wifi, sound, catering);

        var today = DateTime.UtcNow.Date;

        // Допоміжна функція створення бронювання з динамічним розрахунком
        void AddBooking(Room room, DateTime start, DateTime end, params Service[] selectedServices)
        {
            var price = pricingService.CalculateTotalPrice(
                room.BasePricePerHour,
                start,
                end,
                selectedServices.Select(s => s.Price)
            );

            var booking = new Booking
            {
                RoomId = room.Id,
                Room = room,
                StartTime = start,
                EndTime = end,
                TotalPrice = price,
                SelectedServices = selectedServices.Select(s => new BookingSelectedService
                {
                    ServiceId = s.Id,
                    Service = s,
                    Price = s.Price
                }).ToList()
            };

            context.Bookings.Add(booking);
        }

        // 3. Історичні та актуальні тестові бронювання
        AddBooking(roomA, today.AddDays(-5).AddHours(9), today.AddDays(-5).AddHours(12), projector, wifi);
        AddBooking(roomB, today.AddDays(-4).AddHours(10), today.AddDays(-4).AddHours(15), wifi, projector, sound);
        AddBooking(roomC, today.AddDays(-3).AddHours(12), today.AddDays(-3).AddHours(18), wifi, sound, catering, video);
        AddBooking(roomA, today.AddDays(-2).AddHours(14), today.AddDays(-2).AddHours(17), wifi);
        AddBooking(roomB, today.AddDays(-2).AddHours(9), today.AddDays(-2).AddHours(13), projector, video);
        AddBooking(roomC, today.AddDays(-1).AddHours(11), today.AddDays(-1).AddHours(16), wifi, projector, sound, catering);

        AddBooking(roomA, today.AddHours(10), today.AddHours(12), wifi);
        AddBooking(roomB, today.AddHours(14), today.AddHours(17), projector, sound);

        AddBooking(roomB, today.AddDays(1).AddHours(10), today.AddDays(1).AddHours(14), video, wifi);
        AddBooking(roomC, today.AddDays(1).AddHours(13), today.AddDays(1).AddHours(19), catering, sound, projector);

        context.SaveChanges();
    }
}
