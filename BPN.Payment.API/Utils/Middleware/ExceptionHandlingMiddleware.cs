using BPN.Payment.API.Utils.Exceptions;
using System.Net;
using System.Text.Json;

namespace BPN.Payment.API.Utils.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled Exception: {ex.Message}\n{ex.StackTrace}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string errorMessage;

            switch (exception)
            {
                case ProductNotFoundException:
                case OrderNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = exception.Message;
                    break;

                case PaymentFailedException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = exception.Message;
                    break;

                case InsufficientBalanceException:
                    statusCode = HttpStatusCode.PaymentRequired;
                    errorMessage = "Insufficient balance to complete the transaction.";
                    break;

                case BalanceServiceUnavailableException:
                    statusCode = HttpStatusCode.ServiceUnavailable;
                    errorMessage = "Payment service is currently unavailable. Please try again later.";
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorMessage = "Something went wrong. Please try again later.";
                    break;
            }

            var response = new { error = errorMessage };
            var result = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
