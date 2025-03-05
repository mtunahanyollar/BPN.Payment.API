
using BPN.Payment.API.Models;
using BPN.Payment.API.Utils.Constants;

namespace BPN.Payment.API.Services.ProductService
{
    public interface IProductService
    {
        Task<PaginatedResult<Product>> GetProductsAsync(int page, int size);
    }
}