using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;

namespace ClientAppPOSWebAPI.Managers
{
    public class OrderManager
    {
        private readonly OrderService _orderService;
        private readonly ProductsService _productsService;
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePrintService _invoicePrintService;

        public OrderManager(OrderService orderService, ProductsService productsService, InvoiceService invoiceService, InvoicePrintService invoicePrintService)
        {
            _orderService = orderService;
            _productsService = productsService;
            _invoiceService = invoiceService;
            _invoicePrintService = invoicePrintService;
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
                IsDraft = createOrderDto.IsDraft,
                OrderStatus = createOrderDto.OrderStatus ?? "Pending",
                PaymentStatus = createOrderDto.PaymentStatus ?? "Pending"
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

            // Save the order first
            var savedOrder = await _orderService.AddOrderAsync(order);

            // After successful order creation, reduce stock quantities
            try
            {
                foreach (var itemDto in createOrderDto.OrderItems)
                {
                    var stockReduced = await _productsService.ReduceStockQuantityAsync(itemDto.ProductId, itemDto.Quantity);
                    if (!stockReduced)
                    {
                        // Log warning - stock reduction failed but order was created
                        // In production, you might want to handle this differently
                        Console.WriteLine($"Warning: Failed to reduce stock for product {itemDto.ProductId}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the order creation
                // In production, you might want to handle this differently
                Console.WriteLine($"Error reducing stock quantities: {ex.Message}");
            }

            // Check if order was created with completed status and generate invoice automatically
            if (savedOrder != null && 
                !string.IsNullOrEmpty(savedOrder.OrderStatus) && 
                savedOrder.OrderStatus.ToLower() == "completed" &&
                !savedOrder.IsDraft) // Only generate for final orders, not drafts
            {
                try
                {
                    // Automatically generate invoice when order is created as completed
                    var invoice = await _invoiceService.GenerateInvoiceFromOrderAsync(savedOrder.Id);
                    Console.WriteLine($"Invoice automatically generated for new completed order {savedOrder.OrderNumber}");
                    
                    // Automatically print the invoice
                    var printSuccess = await _invoicePrintService.PrintInvoiceAsync(invoice.Id);
                    if (printSuccess)
                    {
                        Console.WriteLine($"Invoice automatically printed for order {savedOrder.OrderNumber}");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Failed to auto-print invoice for order {savedOrder.OrderNumber}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to auto-generate/print invoice for new order {savedOrder.Id}: {ex.Message}");
                }
            }

            return savedOrder;
        }

        public async Task<Order> ReplaceOrderAsync(int id, CreateOrderDto createOrderDto)
        {
            // Get the current order to check status changes and restore stock
            var currentOrder = await _orderService.GetOrderByIdAsync(id);
            if (currentOrder == null)
                return null;

            // Restore stock quantities from the old order items before replacing
            try
            {
                foreach (var orderItem in currentOrder.OrderItems)
                {
                    var stockIncreased = await _productsService.IncreaseStockQuantityAsync(orderItem.ProductId, orderItem.Quantity);
                    if (!stockIncreased)
                    {
                        Console.WriteLine($"Warning: Failed to restore stock for product {orderItem.ProductId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring stock quantities: {ex.Message}");
            }

            // Create new order with provided data
            var order = new Order
            {
                Id = id, // Always use ID from URL parameter, ignore request body ID
                CustomerId = createOrderDto.CustomerId,
                CustomerName = createOrderDto.CustomerName,
                CustomerPhone = createOrderDto.CustomerPhone,
                CustomerEmail = createOrderDto.CustomerEmail,
                PaymentMethod = createOrderDto.PaymentMethod,
                Notes = createOrderDto.Notes,
                IsDraft = createOrderDto.IsDraft,
                OrderStatus = createOrderDto.OrderStatus ?? "Pending",
                PaymentStatus = createOrderDto.PaymentStatus ?? "Pending",
                OrderNumber = currentOrder.OrderNumber, // Keep the same order number
                OrderDate = currentOrder.OrderDate, // Keep the original order date
                CreatedAt = currentOrder.CreatedAt // Keep the original creation date
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

                // Use provided values if available, otherwise calculate
                var unitPrice = itemDto.Price ?? product.Price;
                var itemSubTotal = itemDto.Total ?? (unitPrice * itemDto.Quantity);
                var itemTax = itemDto.Tax ?? (((decimal)(product.TaxRate ?? 0)) / 100m * itemSubTotal);
                var itemDiscount = itemDto.Discount ?? (((decimal)(product.DiscountRate ?? 0)) / 100m * itemSubTotal);

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = itemDto.Name ?? product.Name,
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

            // Use provided totals if available, otherwise use calculated values
            order.SubTotal = createOrderDto.SubTotal ?? subTotal;
            order.TaxAmount = createOrderDto.TaxAmount ?? totalTax;
            order.DiscountAmount = createOrderDto.DiscountAmount ?? totalDiscount;
            order.TotalAmount = createOrderDto.TotalAmount ?? (order.SubTotal + order.TaxAmount - order.DiscountAmount);

            // Reduce stock quantities for the new order items
            try
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var stockReduced = await _productsService.ReduceStockQuantityAsync(orderItem.ProductId, orderItem.Quantity);
                    if (!stockReduced)
                    {
                        Console.WriteLine($"Warning: Insufficient stock for product {orderItem.ProductId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reducing stock quantities: {ex.Message}");
            }

            // Replace the order in the database
            var replacedOrder = await _orderService.ReplaceOrderAsync(id, order);

            // Check if order was replaced with completed status and generate invoice automatically
            if (replacedOrder != null && 
                !string.IsNullOrEmpty(replacedOrder.OrderStatus) && 
                replacedOrder.OrderStatus.ToLower() == "completed" &&
                !replacedOrder.IsDraft) // Only generate for final orders, not drafts
            {
                try
                {
                    // Automatically generate invoice when order is replaced as completed
                    var invoice = await _invoiceService.GenerateInvoiceFromOrderAsync(id);
                    Console.WriteLine($"Invoice automatically generated for replaced completed order {replacedOrder.OrderNumber}");
                    
                    // Automatically print the invoice
                    var printSuccess = await _invoicePrintService.PrintInvoiceAsync(invoice.Id);
                    if (printSuccess)
                    {
                        Console.WriteLine($"Invoice automatically printed for replaced order {replacedOrder.OrderNumber}");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Failed to auto-print invoice for replaced order {replacedOrder.OrderNumber}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to auto-generate/print invoice for replaced order {id}: {ex.Message}");
                }
            }

            return replacedOrder;
        }

        public async Task<Order> UpdateOrderAsync(int id, OrderDto dto)
        {
            // Get the current order to check status changes
            var currentOrder = await _orderService.GetOrderByIdAsync(id);
            if (currentOrder == null)
                return null;

            // Handle IsDraft changes
            if (dto.IsDraft.HasValue)
            {
                // If changing from draft to final order
                if (currentOrder.IsDraft && !dto.IsDraft.Value)
                {
                    // Update status to Pending when converting draft to final
                    if (string.IsNullOrEmpty(dto.OrderStatus))
                        dto.OrderStatus = "Pending";
                    if (string.IsNullOrEmpty(dto.PaymentStatus))
                        dto.PaymentStatus = "Pending";
                }
                // If changing from final to draft order
                else if (!currentOrder.IsDraft && dto.IsDraft.Value)
                {
                    // Update status to Draft when converting final to draft
                    if (string.IsNullOrEmpty(dto.OrderStatus))
                        dto.OrderStatus = "Draft";
                    if (string.IsNullOrEmpty(dto.PaymentStatus))
                        dto.PaymentStatus = "Draft";
                }
            }

            // Check if order status is being changed to cancelled
            if (!string.IsNullOrEmpty(dto.OrderStatus) && 
                dto.OrderStatus.ToLower() == "cancelled" && 
                currentOrder.OrderStatus?.ToLower() != "cancelled")
            {
                // Restore stock quantities when order is cancelled
                try
                {
                    foreach (var orderItem in currentOrder.OrderItems)
                    {
                        var stockIncreased = await _productsService.IncreaseStockQuantityAsync(orderItem.ProductId, orderItem.Quantity);
                        if (!stockIncreased)
                        {
                            Console.WriteLine($"Warning: Failed to restore stock for product {orderItem.ProductId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error restoring stock quantities: {ex.Message}");
                }
            }

            // Update the order
            var updatedOrder = await _orderService.UpdateOrderAsync(id, dto);

            // Check if order was just completed and generate invoice automatically
            if (updatedOrder != null && 
                !string.IsNullOrEmpty(dto.OrderStatus) && 
                dto.OrderStatus.ToLower() == "completed" && 
                currentOrder.OrderStatus?.ToLower() != "completed" &&
                !updatedOrder.IsDraft) // Only generate for final orders, not drafts
            {
                try
                {
                    // Automatically generate invoice when order is completed
                    var invoice = await _invoiceService.GenerateInvoiceFromOrderAsync(id);
                    Console.WriteLine($"Invoice automatically generated for completed order {updatedOrder.OrderNumber}");
                    
                    // Automatically print the invoice
                    var printSuccess = await _invoicePrintService.PrintInvoiceAsync(invoice.Id);
                    if (printSuccess)
                    {
                        Console.WriteLine($"Invoice automatically printed for order {updatedOrder.OrderNumber}");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Failed to auto-print invoice for order {updatedOrder.OrderNumber}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to auto-generate/print invoice for order {id}: {ex.Message}");
                }
            }

            return updatedOrder;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            // Get the order first to restore stock quantities
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return false;

            // Restore stock quantities before deleting the order
            try
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var stockIncreased = await _productsService.IncreaseStockQuantityAsync(orderItem.ProductId, orderItem.Quantity);
                    if (!stockIncreased)
                    {
                        // Log warning - stock restoration failed
                        Console.WriteLine($"Warning: Failed to restore stock for product {orderItem.ProductId}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with order deletion
                Console.WriteLine($"Error restoring stock quantities: {ex.Message}");
            }

            // Delete the order
            return await _orderService.DeleteOrderAsync(id);
        }
    }
}
