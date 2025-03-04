using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.BalanceManagementService;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBalanceManagementService _balanceService;

        public OrderService(ApplicationDbContext context, IBalanceManagementService balanceService)
        {
            _context = context;
            _balanceService = balanceService;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order.Items == null || !order.Items.Any())
            {
                throw new ArgumentException("Order must contain at least one item.");
            }

            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            if (products.Count != order.Items.Count)
            {
                throw new ArgumentException("One or more products are invalid.");
            }

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    item.Price = product.Price;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Use the order's actual ID
            bool fundsReserved = await _balanceService.ReserveFunds(order.Id, order.TotalPrice);

            if (!fundsReserved)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Insufficient balance to reserve funds.");
            }

            return order;
        }

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            bool paymentSuccess = await _balanceService.FinalizePayment(order.Id, order.TotalPrice);
            return paymentSuccess;
        }

    }
}
