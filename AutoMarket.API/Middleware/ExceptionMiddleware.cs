namespace AutoMarket.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => 403,
                KeyNotFoundException        => 404,
                ArgumentException           => 400,
                _                           => 500
            };

            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = context.Response.StatusCode,
                message = context.Response.StatusCode == 500
                    ? "An unexpected error occurred."
                    : ex.Message
            });
        }
    }
}
