# Stock Management in Order API

## Overview
The Order API now includes automatic stock management functionality that reduces product stock quantities when orders are created and restores them when orders are cancelled or deleted.

## Features

### 1. Automatic Stock Reduction on Order Creation
- **When**: An order is successfully created
- **Action**: Stock quantity of each product is reduced by the ordered quantity
- **Implementation**: Happens after order is saved to database
- **Error Handling**: If stock reduction fails, order is still created but warning is logged

### 2. Automatic Stock Restoration on Order Cancellation
- **When**: Order status is changed to "Cancelled"
- **Action**: Stock quantity of each product is restored to its original value
- **Implementation**: Happens before order status is updated
- **Error Handling**: If stock restoration fails, warning is logged but order cancellation continues

### 3. Automatic Stock Restoration on Order Deletion
- **When**: An order is deleted from the system
- **Action**: Stock quantity of each product is restored to its original value
- **Implementation**: Happens before order is deleted
- **Error Handling**: If stock restoration fails, warning is logged but order deletion continues

## Code Implementation

### ProductsService Methods
```csharp
// Reduce stock quantity
public async Task<bool> ReduceStockQuantityAsync(int productId, int quantity)

// Increase stock quantity
public async Task<bool> IncreaseStockQuantityAsync(int productId, int quantity)
```

### OrderManager Integration
```csharp
// In CreateOrderAsync - Stock reduction after order creation
foreach (var itemDto in createOrderDto.OrderItems)
{
    var stockReduced = await _productsService.ReduceStockQuantityAsync(
        itemDto.ProductId, itemDto.Quantity);
}

// In UpdateOrderAsync - Stock restoration on cancellation
if (dto.OrderStatus.ToLower() == "cancelled")
{
    foreach (var orderItem in currentOrder.OrderItems)
    {
        await _productsService.IncreaseStockQuantityAsync(
            orderItem.ProductId, orderItem.Quantity);
    }
}

// In DeleteOrderAsync - Stock restoration before deletion
foreach (var orderItem in order.OrderItems)
{
    await _productsService.IncreaseStockQuantityAsync(
        orderItem.ProductId, orderItem.Quantity);
}
```

## Stock Flow Examples

### Example 1: Complete Order Lifecycle
```
Product: "Apple iPhone 15"
Initial Stock: 50 units

1. Order Created: Quantity = 3
   → Stock: 50 - 3 = 47 units
   → Order Status: "Pending"

2. Order Cancelled: Quantity = 3
   → Stock: 47 + 3 = 50 units
   → Order Status: "Cancelled"
```

### Example 2: Multiple Orders
```
Product: "Samsung Galaxy S24"
Initial Stock: 100 units

1. Order #1: Quantity = 5
   → Stock: 100 - 5 = 95 units

2. Order #2: Quantity = 3
   → Stock: 95 - 3 = 92 units

3. Order #1 Cancelled: Quantity = 5
   → Stock: 92 + 5 = 97 units

4. Order #2 Completed: Quantity = 3
   → Stock: 97 units (no change - order completed)
```

## Error Handling

### Stock Reduction Failures
- **Scenario**: Insufficient stock or product not found
- **Action**: Order is still created, warning is logged
- **Reason**: Order creation should not fail due to stock issues
- **Production Note**: Consider implementing stock reservation system

### Stock Restoration Failures
- **Scenario**: Product not found or database error
- **Action**: Warning is logged, operation continues
- **Reason**: Stock restoration should not block order cancellation/deletion
- **Production Note**: Implement proper logging and monitoring

## Production Considerations

### 1. Stock Reservation System
Consider implementing a stock reservation system where stock is reserved when order is created and only reduced when order is confirmed.

### 2. Transaction Management
In production, consider wrapping stock operations in database transactions to ensure data consistency.

### 3. Logging and Monitoring
Replace `Console.WriteLine` with proper logging framework and implement monitoring for stock operations.

### 4. Stock Validation
Consider adding stock validation before order creation to prevent orders with insufficient stock.

### 5. Audit Trail
Implement audit trail to track all stock changes for compliance and debugging.

## API Endpoints Affected

### POST /api/orders
- **Before**: Only created order
- **After**: Creates order + reduces stock quantities

### PATCH /api/orders/{id}
- **Before**: Only updated order
- **After**: Updates order + restores stock if cancelled

### DELETE /api/orders/{id}
- **Before**: Only deleted order
- **After**: Restores stock + deletes order

## Testing Scenarios

### 1. Stock Reduction Test
- Create order with multiple products
- Verify stock quantities are reduced
- Check database for updated stock values

### 2. Stock Restoration Test
- Cancel an existing order
- Verify stock quantities are restored
- Check database for updated stock values

### 3. Error Handling Test
- Try to create order with non-existent product
- Verify order is created but warning is logged
- Check stock quantities remain unchanged

### 4. Multiple Operations Test
- Create multiple orders
- Cancel some orders
- Delete some orders
- Verify final stock quantities are correct
