namespace ProxarAPI.Middleware;

public class ApiError
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IEnumerable<string>? Errors { get; init; }

    public static ApiError NotFound(string message) => new()
    {
        Type = "not_found",
        Message = message
    };

    public static ApiError BadRequest(string message) => new()
    {
        Type = "bad_request",
        Message = message
    };

    public static ApiError Conflict(string message) => new()
    {
        Type = "conflict",
        Message = message
    };

    public static ApiError Forbidden(string message) => new()
    {
        Type = "forbidden",
        Message = message
    };

    public static ApiError Unauthorized(string message) => new()
    {
        Type = "unauthorized",
        Message = string.IsNullOrEmpty(message) ? "No autenticado" : message
    };

    public static ApiError Validation(IEnumerable<string> errors) => new()
    {
        Type = "validation_error",
        Message = "Los datos enviados no son válidos",
        Errors = errors
    };

    public static ApiError Internal() => new()
    {
        Type = "internal_error",
        Message = "Ocurrió un error interno. Intentá nuevamente."
    };
}
