
using System.Text.Json;
using System.Text;
using Polly.Extensions.Http;
using Polly;
using BPN.Payment.API.Utils.Constants;

namespace BPN.Payment.API.Services.BalanceManagementService
{
    public class BalanceManagementService : IBalanceManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly string _balanceApiBaseUrl;
        private readonly ILogger<BalanceManagementService> _logger;

        public BalanceManagementService(HttpClient httpClient, IConfiguration configuration, ILogger<BalanceManagementService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _balanceApiBaseUrl = configuration["BalanceManagement:ApiBaseUrl"] ?? throw new ArgumentNullException("Balance Management API URL is missing.");
        }

        public async Task<APIResponse> ReserveFunds(int orderId, decimal amount)
        {
            var requestData = new { orderId, amount };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await ExecuteWithRetry(async () => await _httpClient.PostAsync($"{_balanceApiBaseUrl}{BalanceManagementConstants.PreorderEndpoint}", jsonContent));
            return await HandleApiResponse(response, "Funds reserved successfully", "Failed to reserve funds");
        }

        public async Task<APIResponse> FinalizePayment(int orderId, decimal amount)
        {
            var requestData = new { orderId, amount };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await ExecuteWithRetry(async () => await _httpClient.PostAsync($"{_balanceApiBaseUrl}{BalanceManagementConstants.CompletePaymentEndpoint}", jsonContent));
            return await HandleApiResponse(response, "Payment completed successfully", "Failed to complete payment");
        }

        public async Task<APIResponse> RollbackFunds(int orderId, decimal amount)
        {
            var requestData = new { orderId, amount };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await ExecuteWithRetry(async () => await _httpClient.PostAsync($"{_balanceApiBaseUrl}{BalanceManagementConstants.CancelOrderEndpoint}", jsonContent));
            return await HandleApiResponse(response, "Funds rollback successful", "Failed to rollback funds");
        }

        private async Task<HttpResponseMessage> ExecuteWithRetry(Func<Task<HttpResponseMessage>> action)
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for Balance Management API. Waiting {timeSpan.Seconds} seconds before next attempt. Exception: {exception.Exception?.Message}");
                    });

            return await retryPolicy.ExecuteAsync(async () => await action());
        }

        private async Task<APIResponse> HandleApiResponse(HttpResponseMessage response, string successMessage, string failureMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return new APIResponse(true, successMessage);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"{failureMessage}. HTTP {response.StatusCode}, Error: {errorContent}");

            return new APIResponse(false, $"{failureMessage}. {errorContent}");
        }
    }
}
