using DeepMatch.Application.Common.Exceptions;

namespace DeepMatch.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Необработанное исключение: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = new { type = "ValidationError", detail = "Ошибка валидации", errors = validationEx.Errors };
            await context.Response.WriteAsJsonAsync(response);
        }
        else if (exception is NotFoundException notFoundEx)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            var response = new { type = "NotFound", detail = notFoundEx.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
        else if (exception is ForbiddenException forbiddenEx)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            var response = new { type = "Forbidden", detail = forbiddenEx.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
        else if (exception is DailyLimitExceededException limitEx)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var response = new { type = "DailyLimit", detail = limitEx.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var response = new { type = "InternalError", detail = "Произошла внутренняя ошибка сервера" };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
