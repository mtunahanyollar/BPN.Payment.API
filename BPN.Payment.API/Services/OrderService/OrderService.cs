using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.BalanceManagementService;
using BPN.Payment.API.Utils.Enums;
using BPN.Payment.API.Utils.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var reserveResponse = await _balanceService.ReserveFunds(order.Id, order.TotalPrice);
                    if (!reserveResponse.Success)
                    {
                        _logger.LogWarning($"Failed to reserve funds for Order ID {order.Id}: {reserveResponse.Message}");
                        throw new InsufficientBalanceException(order.TotalPrice, reserveResponse.Message); // might be change needed
                    }

                    order.Status = OrderStatus.PaymentReserved;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction Failed: {ex.Message}");
                    await transaction.RollbackAsync(); 
                    throw;
                }
            }

            return order;
        }
        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync()) 
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

                    var paymentResponse = await _balanceService.FinalizePayment(order.Id, order.TotalPrice);
                    if (!paymentResponse.Success)
                    {
                        _logger.LogWarning($"Payment failed for Order ID {order.Id}: {paymentResponse.Message}");

                        // 🔄 Rollback funds because payment failed
                        await _balanceService.RollbackFunds(order.Id, order.TotalPrice);

                        throw new PaymentFailedException(order.Id, order.TotalPrice, paymentResponse.Message);
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
