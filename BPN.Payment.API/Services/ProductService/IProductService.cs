
using BPN.Payment.API.Models;

namespace BPN.Payment.API.Services.ProductService
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}