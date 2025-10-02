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

        public async Task<PagedResult<Product>> GetAllProductsAsync(ProductFilterDto filters)
        {
            var query = _context.Products.AsQueryable();

            // Apply name filter
            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{filters.Name.ToLower()}%"));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(filters.Category))
            {
                query = query.Where(p => p.Category != null && p.Category.ToLower() == filters.Category.ToLower());
            }

            // Apply price range filters
            if (filters.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filters.MaxPrice.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name) // Add ordering for consistent results
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filters.PageNumber,
                PageSize = filters.PageSize
            };
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
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (!string.IsNullOrEmpty(dto.Sku)) product.Sku = dto.Sku;
            if (!string.IsNullOrEmpty(dto.Barcode)) product.Barcode = dto.Barcode;
            if (!string.IsNullOrEmpty(dto.Category)) product.Category = dto.Category;
            if (!string.IsNullOrEmpty(dto.UnitType)) product.UnitType = dto.UnitType;
            if (!string.IsNullOrEmpty(dto.Unit)) product.Unit = dto.Unit;
            if (dto.UnitValue.HasValue) product.UnitValue = dto.UnitValue.Value;
            if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
            if (dto.TaxRate.HasValue) product.TaxRate = dto.TaxRate.Value;
            if (dto.DiscountRate.HasValue) product.DiscountRate = dto.DiscountRate.Value;
            if(dto.IsAvailable != null) product.IsAvailable = dto.IsAvailable ?? false; // Always assign as boolean is non-nullable
            if (dto.ManufactureDate.HasValue) product.ManufactureDate = dto.ManufactureDate.Value;
            if (dto.ExpiryDate.HasValue) product.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.CreatedAt.HasValue) product.CreatedAt = dto.CreatedAt.Value;
            if (dto.UpdatedAt.HasValue) product.UpdatedAt = dto.UpdatedAt.Value;
            if(dto.IsPartialAllowed != null) product.IsPartialAllowed = dto.IsPartialAllowed ?? false;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> ReduceStockQuantityAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            // Check if there's enough stock
            if (product.StockQuantity.HasValue && product.StockQuantity.Value < quantity)
                return false;

            // Reduce stock quantity
            if (product.StockQuantity.HasValue)
                product.StockQuantity -= quantity;
            else
                product.StockQuantity = 0;

            // Update the product
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncreaseStockQuantityAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            // Increase stock quantity
            if (product.StockQuantity.HasValue)
                product.StockQuantity += quantity;
            else
                product.StockQuantity = quantity;

            // Update the product
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
