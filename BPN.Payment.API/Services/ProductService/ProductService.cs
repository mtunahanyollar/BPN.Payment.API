using BPN.Payment.API.Data;
using BPN.Payment.API.Models;
using BPN.Payment.API.Services.ProductService;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }
    }
}
