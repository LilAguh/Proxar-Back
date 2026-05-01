using System.Net;
using System.Text.Json;
using Exceptions;
using FluentValidation;

namespace ProxarAPI.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiError.NotFound(ex.Message)
            ),
            BusinessRuleException ex => (
                HttpStatusCode.BadRequest,
                ApiError.BadRequest(ex.Message)
            ),
            ConflictException ex => (
                HttpStatusCode.Conflict,
                ApiError.Conflict(ex.Message)
            ),
            ForbiddenException ex => (
                HttpStatusCode.Forbidden,
                ApiError.Forbidden(ex.Message)
            ),
            ValidationException ex => (
                HttpStatusCode.BadRequest,
                ApiError.Validation(ex.Errors.Select(e => e.ErrorMessage))
            ),
            UnauthorizedAccessException ex => (
                HttpStatusCode.Unauthorized,
                ApiError.Unauthorized(ex.Message)
            ),
            KeyNotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiError.NotFound(ex.Message)
            ),
            InvalidOperationException ex => (
                HttpStatusCode.BadRequest,
                ApiError.BadRequest(ex.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiError.Internal()
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception [{Status}]: {Message}", (int)statusCode, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
