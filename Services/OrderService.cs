using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientAppPOSWebAPI.Services
{
    public class OrderService
    {
        private readonly POSDbContext _context;

        public OrderService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(OrderFilterDto filters)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.OrderNumber))
                query = query.Where(o => o.OrderNumber.Contains(filters.OrderNumber));

            if (!string.IsNullOrEmpty(filters.CustomerName))
                query = query.Where(o => o.CustomerName != null && o.CustomerName.Contains(filters.CustomerName));

            if (!string.IsNullOrEmpty(filters.CustomerPhone))
                query = query.Where(o => o.CustomerPhone != null && o.CustomerPhone.Contains(filters.CustomerPhone));

            if (!string.IsNullOrEmpty(filters.OrderStatus))
                query = query.Where(o => o.OrderStatus == filters.OrderStatus);

            if (!string.IsNullOrEmpty(filters.PaymentStatus))
                query = query.Where(o => o.PaymentStatus == filters.PaymentStatus);

            // Handle date range filtering - support both DateTime and DateTimeOffset
            if (filters.StartDate.HasValue)
                query = query.Where(o => o.OrderDate >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(o => o.OrderDate <= filters.EndDate.Value);

            // Handle DateTimeOffset date range filtering
            if (filters.OrderStartDate.HasValue)
                query = query.Where(o => o.OrderDate >= filters.OrderStartDate.Value.DateTime);

            if (filters.OrderEndDate.HasValue)
                query = query.Where(o => o.OrderDate <= filters.OrderEndDate.Value.DateTime);

            if (filters.MinTotal.HasValue)
                query = query.Where(o => o.TotalAmount >= filters.MinTotal.Value);

            if (filters.MaxTotal.HasValue)
                query = query.Where(o => o.TotalAmount <= filters.MaxTotal.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            // Generate order number if not provided
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = await GenerateOrderNumberAsync();
            }

            order.OrderDate = DateTime.Now;
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            // Set default status if not provided
            if (string.IsNullOrEmpty(order.OrderStatus))
                order.OrderStatus = "Pending";

            if (string.IsNullOrEmpty(order.PaymentStatus))
                order.PaymentStatus = "Pending";

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> UpdateOrderAsync(int id, OrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return null;

            // Update properties if provided
            if (dto.CustomerName != null)
                order.CustomerName = dto.CustomerName;

            if (dto.CustomerPhone != null)
                order.CustomerPhone = dto.CustomerPhone;

            if (dto.CustomerEmail != null)
                order.CustomerEmail = dto.CustomerEmail;

            if (dto.SubTotal.HasValue)
                order.SubTotal = dto.SubTotal.Value;

            if (dto.TaxAmount.HasValue)
                order.TaxAmount = dto.TaxAmount.Value;

            if (dto.DiscountAmount.HasValue)
                order.DiscountAmount = dto.DiscountAmount.Value;

            if (dto.TotalAmount.HasValue)
                order.TotalAmount = dto.TotalAmount.Value;

            if (dto.PaymentMethod != null)
                order.PaymentMethod = dto.PaymentMethod;

            if (dto.PaymentStatus != null)
                order.PaymentStatus = dto.PaymentStatus;

            if (dto.OrderStatus != null)
                order.OrderStatus = dto.OrderStatus;

            if (dto.Notes != null)
                order.Notes = dto.Notes;

            if (dto.CompletedDate.HasValue)
                order.CompletedDate = dto.CompletedDate.Value;

            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow.Date;
            var count = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .CountAsync();

            return $"ORD-{today:yyyyMMdd}-{count + 1:D4}";
        }
    }
}
