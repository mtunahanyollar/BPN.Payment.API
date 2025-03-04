namespace BPN.Payment.API.Services.BalanceManagementService
{
    public interface IBalanceManagementService
    {
        Task<bool> ReserveFunds(int orderId, decimal amount);
        Task<bool> FinalizePayment(int orderId, decimal amount); //#combeack ? change might be needed.....
    }
}
