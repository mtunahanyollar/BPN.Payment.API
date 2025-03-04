namespace BPN.Payment.API.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(int orderId)
            : base($"Order with ID {orderId} not found.") { }
    }
}
