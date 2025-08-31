using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;

namespace ClientAppPOSWebAPI.Managers
{
    public class OrderManager
    {
        private readonly OrderService _orderService;
        private readonly ProductsService _productsService;

        public OrderManager(OrderService orderService, ProductsService productsService)
        {
            _orderService = orderService;
            _productsService = productsService;
        }

        public async Task<Order?> GetOrderAsync(int id)
        {
            return await _orderService.GetOrderByIdAsync(id);
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(OrderFilterDto filters)
        {
            return await _orderService.GetAllOrdersAsync(filters);
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // Create new order
            var order = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                CustomerName = createOrderDto.CustomerName,
                CustomerPhone = createOrderDto.CustomerPhone,
                CustomerEmail = createOrderDto.CustomerEmail,
                PaymentMethod = createOrderDto.PaymentMethod,
                Notes = createOrderDto.Notes,
                OrderStatus = "Pending",
                PaymentStatus = "Pending"
            };

            // Process order items and calculate totals
            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal totalDiscount = 0;

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var product = await _productsService.GetProductByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found");

                var unitPrice = product.Price;   // Likely decimal
                var itemSubTotal = unitPrice * itemDto.Quantity; // OK if Quantity is int
                var itemTax = ((decimal)(product.TaxRate ?? 0)) / 100m * itemSubTotal;
                var itemDiscount = ((decimal)(product.DiscountRate ?? 0)) / 100m * itemSubTotal;


                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,
                    Quantity = itemDto.Quantity,
                    SubTotal = itemSubTotal,
                    TaxAmount = itemTax,
                    DiscountAmount = itemDiscount,
                    TotalAmount = itemSubTotal + itemTax - itemDiscount,
                    Notes = itemDto.Notes
                };

                order.OrderItems.Add(orderItem);

                subTotal += itemSubTotal;
                totalTax += itemTax;
                totalDiscount += itemDiscount;
            }

            order.SubTotal = subTotal;
            order.TaxAmount = totalTax;
            order.DiscountAmount = totalDiscount;
            order.TotalAmount = subTotal + totalTax - totalDiscount;

            return await _orderService.AddOrderAsync(order);
        }

        public async Task<Order> UpdateOrderAsync(int id, OrderDto dto)
        {
            // Add any business logic here if needed
            return await _orderService.UpdateOrderAsync(id, dto);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            // Add any business logic here if needed
            return await _orderService.DeleteOrderAsync(id);
        }
    }
}
