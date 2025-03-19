using System.Net;
using System.Text.Json;

namespace car.api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");

            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ArgumentException argEx:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new { error = argEx.Message });
                    break;

                case KeyNotFoundException:
                    code = HttpStatusCode.NotFound;
                    result = JsonSerializer.Serialize(new { error = "The requested resource was not found." });
                    break;

                case UnauthorizedAccessException:
                    code = HttpStatusCode.Forbidden;
                    result = JsonSerializer.Serialize(new { error = "You do not have permission to access this resource." });
                    break;

                case System.ComponentModel.DataAnnotations.ValidationException valEx:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new { error = valEx.Message });
                    break;

                default:
                    result = _environment.IsDevelopment()
                        ? JsonSerializer.Serialize(new { error = exception.Message, stackTrace = exception.StackTrace })
                        : JsonSerializer.Serialize(new { error = "An unexpected error occurred. Please try again later." });
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}