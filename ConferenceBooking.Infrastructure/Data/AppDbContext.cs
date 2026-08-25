using ConferenceBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Data;

/// <summary>
/// Основний контекст бази даних Entity Framework Core для системи бронювання конференц-залів.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Конструктор контексту БД з параметрами підключення.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Таблиця конференц-залів.
    /// </summary>
    public DbSet<Room> Rooms => Set<Room>();

    /// <summary>
    /// Довідник додаткових послуг (Wi-Fi, проєктор тощо).
    /// </summary>
    public DbSet<Service> Services => Set<Service>();

    /// <summary>
    /// Таблиця оформлених бронювань.
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Таблиця обраних послуг у бронюваннях із зафіксованою історичною вартістю.
    /// </summary>
    public DbSet<BookingSelectedService> BookingSelectedServices => Set<BookingSelectedService>();

    /// <summary>
    /// Проміжна таблиця зв'язку доступних послуг для конференц-залів.
    /// </summary>
    public DbSet<RoomServiceItem> RoomServices => Set<RoomServiceItem>();

    /// <summary>
    /// Налаштування моделі даних, зв'язків та обмежень цілісності.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Проміжна таблиця зв'язку many-to-many між залами та доступними послугами
        modelBuilder.Entity<RoomServiceItem>(entity =>
        {
            entity.HasKey(rs => new { rs.RoomId, rs.ServiceId });

            entity.HasOne(rs => rs.Room)
                .WithMany(r => r.RoomServices)
                .HasForeignKey(rs => rs.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rs => rs.Service)
                .WithMany(s => s.RoomServices)
                .HasForeignKey(rs => rs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Обрані послуги у бронюванні (з фіксацією ціни на момент замовлення)
        modelBuilder.Entity<BookingSelectedService>(entity =>
        {
            entity.HasKey(bs => bs.Id);

            entity.HasOne(bs => bs.Booking)
                .WithMany(b => b.SelectedServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Забороняємо каскадне видалення послуги з довідника, якщо вона вже фігурує в історії бронювань
            entity.HasOne(bs => bs.Service)
                .WithMany()
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Зв'язок бронювання із залом
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
