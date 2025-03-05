using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.ProductService;
using BPN.Payment.API.Utils.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Controllers
{
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
            var result = await _productService.GetProductsAsync(page, size);
            return Ok(result);
        }
    }
}
