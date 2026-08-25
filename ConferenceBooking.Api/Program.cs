using System.Reflection;
using ConferenceBooking.Api.Middleware;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Services;
using ConferenceBooking.Infrastructure.Data;
using ConferenceBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Conference Room Booking API",
        Version = "v1",
        Description = "RESTful API для управління конференц-залами, бронюванням, динамічним розрахунком вартості оренди та аналітичною звітністю.\n\n" +
                      "### Основні можливості:\n" +
                      "- **Управління залами та послугами**: Створення, редагування, перегляд і видалення конференц-залів та прив'язка додаткових сервісів.\n" +
                      "- **Пошук вільних залів**: Фільтрація залів за часовим проміжком і мінімальною місткістю без часових колізій.\n" +
                      "- **Динамічне ціноутворення**: Погодинний розрахунок за коефіцієнтами доби (ранок: 0.9, день/ніч: 1.0, пік: 1.15, вечір: 0.8) + фіксовані послуги.\n" +
                      "- **Аналітичні звіти**: Виручка, рівень завантаженості залів та рейтинг популярності послуг за період.",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Conference Booking Team"
        }
    });

    // Підключаємо XML-документацію для всіх шарів: API, Application, Domain
    var assemblies = new[]
    {
        Assembly.GetExecutingAssembly(),
        typeof(ConferenceBooking.Application.DTOs.RoomDto).Assembly,
        typeof(ConferenceBooking.Domain.Entities.Room).Assembly
    };

    foreach (var assembly in assemblies)
    {
        var xmlFilename = $"{assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=conference.db"));

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

builder.Services.AddScoped<IPricingDomainService, PricingDomainService>();

builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

SeedData.Initialize(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
