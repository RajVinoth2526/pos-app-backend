using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientAppPOSWebAPI.Services
{
    public class ProductsService
    {
        private readonly POSDbContext _context;

        public ProductsService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            if (product == null)
            {
                return null;  // Return null if the product is invalid (optional).
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;  // Return the created product with the generated Id.
        }

        public async Task<Product> UpdateProductAsync(int id, ProductDto dto)
        {
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            // Assuming `productDto` is an instance of ProductDto and `product` is an instance of the Product model
            if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) product.Description = dto.Description;
            if (dto.Image != null) product.Image = dto.Image;
            if (dto.Price != 0) product.Price = dto.Price;
            if (!string.IsNullOrEmpty(dto.Sku)) product.Sku = dto.Sku;
            if (!string.IsNullOrEmpty(dto.Barcode)) product.Barcode = dto.Barcode;
            if (!string.IsNullOrEmpty(dto.Category)) product.Category = dto.Category;
            if (!string.IsNullOrEmpty(dto.UnitType)) product.UnitType = dto.UnitType;
            if (!string.IsNullOrEmpty(dto.Unit)) product.Unit = dto.Unit;
            if (dto.UnitValue != null) product.UnitValue = dto.UnitValue;
            if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
            if (!string.IsNullOrEmpty(dto.Unit)) product.Unit = dto.Unit;
            if (dto.TaxRate.HasValue) product.TaxRate = dto.TaxRate.Value;
            if (dto.DiscountRate.HasValue) product.DiscountRate = dto.DiscountRate;
            product.IsAvailable = dto.IsAvailable; // Always assign as boolean is non-nullable
            if (dto.ManufactureDate.HasValue) product.ManufactureDate = dto.ManufactureDate.Value;
            if (dto.ExpiryDate.HasValue) product.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.CreatedAt.HasValue) product.CreatedAt = dto.CreatedAt.Value;
            if (dto.UpdatedAt.HasValue) product.UpdatedAt = dto.UpdatedAt.Value;

            await _context.SaveChangesAsync();
            return product;
        }

    }
}
