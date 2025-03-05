namespace BPN.Payment.API.Utils.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public decimal Amount { get; }

        public InsufficientBalanceException(decimal amount, string message)
            : base(message)
        {
            Amount = amount;
        }
    }

}
