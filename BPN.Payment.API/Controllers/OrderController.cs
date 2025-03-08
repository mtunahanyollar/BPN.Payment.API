using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.OrderService;
using BPN.Payment.API.Utils.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(order);
                return CreatedAtAction(nameof(CreateOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogWarning($"Create Order Failed: {ex.Message}");
                return NotFound(new { error = ex.Message });
            }
            catch (InsufficientBalanceException ex)
            {
                _logger.LogWarning($"Create Order Failed: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return StatusCode(500, new { error = "Something went wrong. Please try again later." });
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                bool success = await _orderService.CompleteOrderAsync(id);
                if (!success)
                {
                    return BadRequest(new { error = "Payment failed or order not in valid state for completion." });
                }
                return Ok(new { message = "Order completed successfully." });
            }
            catch (OrderNotFoundException ex)
            {
                _logger.LogWarning($"Complete Order Failed: {ex.Message}");
                return NotFound(new { error = ex.Message });
            }
            catch (PaymentFailedException ex)
            {
                _logger.LogWarning($"Complete Order Failed: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return StatusCode(500, new { error = "Something went wrong. Please try again later." });
            }
        }

    }
}
