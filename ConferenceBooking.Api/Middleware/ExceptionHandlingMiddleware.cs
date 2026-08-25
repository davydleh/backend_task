using System.Net;
using System.Text.Json;
using ConferenceBooking.Domain.Exceptions;

namespace ConferenceBooking.Api.Middleware;

/// <summary>
/// Глобальний middleware для перехоплення доменних та системних винятків із формуванням стандартизованої JSON-відповіді з відповідним HTTP статус-кодом.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Ініціалізує новий екземпляр middleware обробки винятків.
    /// </summary>
    /// <param name="next">Наступний делегат у конвеєрі HTTP-запиту.</param>
    /// <param name="logger">Логер для запису інформації про помилки.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Виконує обробку HTTP-запиту з перехопленням винятків.
    /// </summary>
    /// <param name="context">Контекст поточного HTTP-запиту.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Ресурс не знайдено: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(ex, "Помилка валідації доменної моделі: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (RoomUnavailableException ex)
        {
            _logger.LogWarning(ex, "Зал недоступний у вказаний проміжок: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некоректний аргумент: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Неприпустима операція: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Виникла непередбачена внутрішня помилка сервера");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "Виникла неочікувана помилка сервера.");
        }
    }

    /// <summary>
    /// Формує та записує уніфіковану JSON-відповідь про помилку.
    /// </summary>
    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            statusCode = (int)statusCode,
            error = message
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
