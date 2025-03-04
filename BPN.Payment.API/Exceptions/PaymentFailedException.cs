namespace BPN.Payment.API.Exceptions
{
    public class PaymentFailedException : Exception
    {
        public PaymentFailedException(int orderId, decimal amount)
            : base($"Payment failed for Order ID {orderId} with amount {amount}.") { }
    }
}
