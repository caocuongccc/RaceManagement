using System.Net;
using System.Text.Json;

namespace RaceManagement.API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case ArgumentException:
                    errorResponse.Message = exception.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case InvalidOperationException:
                    errorResponse.Message = exception.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case FileNotFoundException:
                    errorResponse.Message = "Required file not found";
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    errorResponse.Message = "Access denied";
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    errorResponse.Message = "An internal server error occurred";
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.StatusCode = errorResponse.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
