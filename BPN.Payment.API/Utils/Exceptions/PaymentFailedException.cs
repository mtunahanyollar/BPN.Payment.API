namespace BPN.Payment.API.Utils.Exceptions
{
    public class PaymentFailedException : Exception
    {
        public int OrderId { get; }
        public decimal Amount { get; }

        public PaymentFailedException(int orderId, decimal amount, string message)
            : base(message)
        {
            OrderId = orderId;
            Amount = amount;
        }
    }

}
