using Microsoft.EntityFrameworkCore;
using SharedLayer.Nlog;
using System.Net;

namespace JourneyAPI.Middlewares
{
    public class ExceptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _logger;

        public ExceptionsMiddleware(RequestDelegate next, ILoggerService loggerService)
        {
            _next = next;
            _logger = loggerService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var cancellationToken = httpContext.RequestAborted;
            try
            {
                await _next(httpContext);
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex);
                await HandleException(httpContext, ex);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex);
                await HandleConcurrencyException(httpContext, ex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                await HandleException(httpContext, ex);
            }
        }
        private async Task HandleException(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(new ErrorInfo()
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            }.ToString());
        }

        private async Task HandleConcurrencyException(HttpContext context, DbUpdateConcurrencyException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsync(new ErrorInfo()
            {
                StatusCode = context.Response.StatusCode,
                Message = "A concurrency conflict occurred. Please try again."
            }.ToString());
        }
    }
}
