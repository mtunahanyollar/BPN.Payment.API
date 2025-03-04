
using System.Text.Json;
using System.Text;
using Polly.Extensions.Http;
using Polly;

namespace BPN.Payment.API.Services.BalanceManagementService
{
    public class BalanceManagementService : IBalanceManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BalanceManagementService> _logger;
        private readonly string _balanceApiBaseUrl;

        public BalanceManagementService(HttpClient httpClient, IConfiguration configuration, ILogger<BalanceManagementService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _balanceApiBaseUrl = configuration["BalanceManagement:ApiBaseUrl"] ?? throw new ArgumentNullException("Balance Management API URL is missing.");
        }

        public async Task<bool> ReserveFunds(int orderId, decimal amount)
        {
            var requestData = new { orderId, amount };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            return await ExecuteWithRetry(async () =>
            {
                var response = await _httpClient.PostAsync($"{_balanceApiBaseUrl}/api/balance/preorder", jsonContent);
                return response.IsSuccessStatusCode;
            });
        }

        public async Task<bool> FinalizePayment(int orderId, decimal amount)
        {
            var requestData = new { orderId, amount };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            return await ExecuteWithRetry(async () =>
            {
                var response = await _httpClient.PostAsync($"{_balanceApiBaseUrl}/api/balance/complete", jsonContent);
                return response.IsSuccessStatusCode;
            });
        }

        //#CB -> Make this another service...
        private async Task<bool> ExecuteWithRetry(Func<Task<bool>> action)
        {
            var retryPolicy = Policy
         .Handle<Exception>() // Handles transient failures
         .WaitAndRetryAsync(
             3, // Number of retries
             retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
             (exception, timeSpan, retryCount, context) =>
             {
                 _logger.LogError($"Retry {retryCount} for Balance Management API. Waiting {timeSpan.Seconds} seconds before next attempt. Exception: {exception.Message}");
             });

            return await retryPolicy.ExecuteAsync(async () => await action());
        }
    }
}
