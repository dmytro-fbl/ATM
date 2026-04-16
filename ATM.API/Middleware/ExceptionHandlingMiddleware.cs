using System.Net;
using System.Text.Json;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;

namespace ATM.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAtmOperationLogRepository logRepo)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, logRepo);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, IAtmOperationLogRepository logRepo)
        {
            var log = new AtmOperationLog
            {
                Id = Guid.NewGuid(),
                LogDate = DateTime.UtcNow,
                LogLevel = "Error",
                Message = $"Критична помилка сервера: {exception.Message}",
                StackTrace = exception.StackTrace,
                CardId = null
            };

            await logRepo.AddAsync(log);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new { error = exception.Message };

            var jsonResponse = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
