namespace BPN.Payment.API.Exceptions
{
    public class BalanceServiceUnavailableException : Exception
    {
        public BalanceServiceUnavailableException()
            : base("Balance Management Service is currently unavailable.") { }
    }
}
