using BPN.Payment.API.Utils.Constants;

namespace BPN.Payment.API.Services.BalanceManagementService
{
    public interface IBalanceManagementService
    {
        Task<APIResponse> ReserveFunds(int orderId, decimal amount);
        Task<APIResponse> FinalizePayment(int orderId, decimal amount);
        Task<APIResponse> RollbackFunds(int orderId, decimal amount);
    }
}
