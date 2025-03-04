using BPN.Payment.API.Models;

namespace BPN.Payment.API.Services.OrderService
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> CompleteOrderAsync(int orderId);
    }
}
