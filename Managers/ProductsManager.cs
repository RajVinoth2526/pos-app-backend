using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;

namespace ClientAppPOSWebAPI.Managers
{
    public class ProductsManager
    {
        private readonly ProductsService _productService;

        public ProductsManager(ProductsService productService)
        {
            _productService = productService;
        }

        public async Task<Product> GetProductAsync(int id)
        {
            return await _productService.GetProductByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productService.GetAllProductsAsync();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            // Add any business logic here if needed
            return await _productService.AddProductAsync(product);
        }
        public async Task<Product> UpdateProduct(int id, ProductDto dto)
        {
            // Add any business logic here if needed
            return await _productService.UpdateProductAsync(id, dto);
        }

    }
}
