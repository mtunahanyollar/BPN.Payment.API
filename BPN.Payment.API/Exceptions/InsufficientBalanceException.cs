namespace BPN.Payment.API.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(decimal amount)
            : base($"Insufficient balance to reserve funds of {amount}.") { }
    }
}
