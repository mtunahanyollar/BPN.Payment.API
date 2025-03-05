using BPN.Payment.API.Data;
using BPN.Payment.API.Enums;
using BPN.Payment.API.Exceptions;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.BalanceManagementService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBalanceManagementService _balanceService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IBalanceManagementService balanceService, ILogger<OrderService> logger)
        {
            _context = context;
            _balanceService = balanceService;
            _logger = logger;
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
                throw new ProductNotFoundException(products.FirstOrDefault(p => !productIds.Contains(p.Id))?.Id ?? 0);
            }

            // Assign product prices to the order
            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    item.Price = product.Price;
                }
            }

            order.Status = OrderStatus.PendingPayment;

            using (var transaction = await _context.Database.BeginTransactionAsync()) // Start Transaction
            {
                try
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    bool fundsReserved = await _balanceService.ReserveFunds(order.Id, order.TotalPrice);
                    if (!fundsReserved)
                    {
                        throw new InsufficientBalanceException(order.TotalPrice);
                    }

                    order.Status = OrderStatus.PaymentReserved;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync(); // ✅ Commit if everything succeeds
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction Failed: {ex.Message}");
                    await transaction.RollbackAsync(); // ❌ Rollback everything if any step fails
                    throw;
                }
            }

            return order;
        }
        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync()) // Start Transaction
            {
                try
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order == null)
                    {
                        throw new OrderNotFoundException(orderId);
                    }

                    if (order.Status != OrderStatus.PaymentReserved)
                    {
                        _logger.LogWarning($"Order ID {order.Id} is not in a valid state for payment completion.");
                        return false;
                    }

                    bool paymentSuccess = await _balanceService.FinalizePayment(order.Id, order.TotalPrice);

                    if (!paymentSuccess)
                    {
                        throw new PaymentFailedException(order.Id, order.TotalPrice);
                    }

                    order.Status = OrderStatus.PaymentCompleted;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync(); 
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction Failed: {ex.Message}");
                    await transaction.RollbackAsync(); 
                    throw;
                }
            }
        }
    }
}
