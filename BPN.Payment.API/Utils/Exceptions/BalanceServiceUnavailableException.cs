namespace BPN.Payment.API.Utils.Exceptions
{
    public class BalanceServiceUnavailableException : Exception
    {
        public BalanceServiceUnavailableException()
            : base("Balance Management Service is currently unavailable.") { }
    }
}
