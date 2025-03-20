using BPN.Payment.API.Controllers;
using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.ProductService;
using BPN.Payment.API.Utils.Constants;
using BPN.Payment.API.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BPN.Payment.API.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public ProductService(ApplicationDbContext context, IMemoryCache cache, ILogger<ProductsController> logger)
        {
            _logger = logger;
            _cache = cache;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<PaginatedResult<Product>> GetProductsAsync(int page, int size)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            if (!_cache.TryGetValue("products_cache", out List<Product> products))
            {
                products = await _context.Products.AsNoTracking().ToListAsync();

                //5 mins
                _cache.Set("products_cache", products, _cacheDuration);
            }
            try
            {
                var totalItems = products.Count;
                var paginatedProducts = products
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToList();

                return new PaginatedResult<Product>(paginatedProducts, totalItems, page, size);
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductController db error: " + ex.Message);
                throw new DatabaseException();
            }

        }

    }
}
