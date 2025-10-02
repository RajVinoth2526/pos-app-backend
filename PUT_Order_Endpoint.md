# PUT Order Endpoint Documentation

## Overview
The PUT endpoint allows you to completely replace an existing order with a new order object. This is different from PATCH which only updates specific fields.

## Endpoint
```http
PUT /api/orders/{id}
```

## Request Body
The request body should contain a complete `CreateOrderDto` object. **Note: The `id` field in the request body is ignored - the ID from the URL parameter is always used.**

```json
{
  "customerId": 1,
  "customerName": "John Doe",
  "customerPhone": "123-456-7890",
  "customerEmail": "john@example.com",
  "paymentMethod": "Cash",
  "orderStatus": "completed",
  "paymentStatus": "paid",
  "isDraft": false,
  "notes": "Updated order",
  "orderItems": [
    {
      "productId": 1,
      "quantity": 2,
      "notes": "Updated item"
    },
    {
      "productId": 2,
      "quantity": 1,
      "notes": "New item"
    }
  ]
}
```

## Behavior

### What Gets Replaced
- ✅ **All order properties** (customer info, status, amounts, etc.)
- ✅ **All order items** (completely replaced)
- ✅ **Calculated totals** (recalculated based on new items)

### What Gets Preserved
- ✅ **Order ID** (remains the same)
- ✅ **Order Number** (remains the same)
- ✅ **Order Date** (original creation date)
- ✅ **Created Date** (original creation timestamp)

### What Gets Updated
- ✅ **Updated Date** (set to current timestamp)
- ✅ **Stock Quantities** (restored from old items, reduced for new items)

## Stock Management
The PUT operation handles stock management intelligently:

1. **Restore Stock**: First restores stock quantities from the old order items
2. **Reduce Stock**: Then reduces stock quantities for the new order items
3. **Error Handling**: Continues operation even if stock adjustments fail (with warnings)

## Example Usage

### Replace an Existing Order
```bash
curl -X PUT "https://localhost:44376/api/orders/123" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "customerId": 1,
    "customerName": "Jane Smith",
    "customerPhone": "987-654-3210",
    "customerEmail": "jane@example.com",
    "paymentMethod": "Credit Card",
    "orderStatus": "completed",
    "paymentStatus": "paid",
    "isDraft": false,
    "notes": "Completely updated order",
    "orderItems": [
      {
        "productId": 3,
        "quantity": 1,
        "notes": "New product"
      }
    ]
  }'
```

### Response
```json
{
  "success": true,
  "data": {
    "id": 123,
    "orderNumber": "ORD-20250920-001",
    "customerId": 1,
    "customerName": "Jane Smith",
    "customerPhone": "987-654-3210",
    "customerEmail": "jane@example.com",
    "subTotal": 50.00,
    "taxAmount": 5.00,
    "totalAmount": 55.00,
    "paymentMethod": "Credit Card",
    "orderStatus": "completed",
    "paymentStatus": "paid",
    "isDraft": false,
    "notes": "Completely updated order",
    "orderDate": "2025-09-20T10:00:00Z",
    "createdAt": "2025-09-20T10:00:00Z",
    "updatedAt": "2025-09-20T19:30:00Z",
    "orderItems": [
      {
        "id": 456,
        "orderId": 123,
        "productId": 3,
        "productName": "New Product",
        "unitPrice": 50.00,
        "quantity": 1,
        "subTotal": 50.00,
        "taxAmount": 5.00,
        "totalAmount": 55.00,
        "notes": "New product"
      }
    ]
  }
}
```

## Error Handling

### 400 Bad Request
- Missing or invalid request body
- No order items provided
- Product not found
- Invalid product ID

### 404 Not Found
- Order with specified ID doesn't exist

### 500 Internal Server Error
- Database errors
- Unexpected system errors

## Comparison: PUT vs PATCH

| Aspect | PUT | PATCH |
|--------|-----|-------|
| **Purpose** | Complete replacement | Partial update |
| **Request Body** | Full CreateOrderDto | Partial OrderDto |
| **Order Items** | Completely replaced | Not updated |
| **Stock Management** | Full restore/reduce | Status-based only |
| **Use Case** | Major order changes | Status updates, minor changes |

## Best Practices

1. **Use PUT when**:
   - Changing order items completely
   - Major order modifications
   - Need to recalculate totals

2. **Use PATCH when**:
   - Updating order status only
   - Changing customer info
   - Minor field updates

3. **Always include**:
   - All required fields
   - At least one order item
   - Valid product IDs

4. **Stock considerations**:
   - PUT handles stock automatically
   - Monitor stock warnings in logs
   - Consider stock availability before replacement

## Implementation Details

### OrderManager.ReplaceOrderAsync()
- Retrieves current order
- Restores stock from old items
- Creates new order object
- Calculates new totals
- Reduces stock for new items
- Calls service layer for database update

### OrderService.ReplaceOrderAsync()
- Removes existing order items
- Updates all order properties
- Adds new order items
- Saves changes to database
- Returns updated order with items

The PUT endpoint provides a complete order replacement capability while maintaining data integrity and proper stock management! 🎉
