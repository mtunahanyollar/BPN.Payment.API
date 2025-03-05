namespace BPN.Payment.API.Utils.Constants
{
    public static class BalanceManagementConstants
    {
        public const string PreorderEndpoint = "/api/balance/preorder";
        public const string CompletePaymentEndpoint = "/api/balance/complete";
        public const string CancelOrderEndpoint = "/api/balance/cancel";
    }
    public class APIResponse
    {
        public bool Success { get; }
        public string Message { get; }

        public APIResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
