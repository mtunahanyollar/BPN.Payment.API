using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.ProductService;
using BPN.Payment.API.Utils.Constants;
using BPN.Payment.API.Utils.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Controllers
{
    //[Authorize]
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Product>>> GetProducts(
      [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var result = await _productService.GetProductsAsync(page, size);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (DatabaseException ex)
            {
                return StatusCode(500, new { error = "Database error occurred. Please try again later." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled Error in GetProductsAsync: {ex.Message}");
                return StatusCode(500, new { error = "Something went wrong. Please try again later." });
            }
        }
    }
}
