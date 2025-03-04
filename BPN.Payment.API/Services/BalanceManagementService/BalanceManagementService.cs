
using System.Text.Json;
using System.Text;

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
            _balanceApiBaseUrl = configuration["BalanceManagement:ApiBaseUrl"] ?? throw new ArgumentNullException("Balance Management API URL is missing in configuration.");
        }

        public async Task<bool> ReserveFunds(int orderId, decimal amount)
        {
            //#comeback -> create a specific model for this.
            var requestData = new
            {
                orderId = orderId,
                amount = amount
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            try
            {
                //#comeback -> get rid of hardcoded endpoints.....
                var response = await _httpClient.PostAsync($"{_balanceApiBaseUrl}/api/balance/preorder", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                _logger.LogError($"Failed to reserve funds. Response: {await response.Content.ReadAsStringAsync()}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reserving funds: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> FinalizePayment(int orderId, decimal amount)
        {
            //#comeback -> create a specific model for this.
            var requestData = new
            {
                orderId = orderId,
                amount = amount
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            try
            {
                //#comeback -> get rid of hardcoded endpoints.....
                var response = await _httpClient.PostAsync($"{_balanceApiBaseUrl}/api/balance/complete", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                _logger.LogError($"Failed to finalize payment. Response: {await response.Content.ReadAsStringAsync()}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finalizing payment: {ex.Message}");
                return false;
            }
        }
    }
}
