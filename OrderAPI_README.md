# Order API Documentation

This document describes the Order API endpoints for the POS system.

## Models

### Order
- `Id`: Unique identifier
- `OrderNumber`: Auto-generated order number (format: ORD-YYYYMMDD-XXXX)
- `CustomerId`: Customer identifier
- `CustomerName`: Customer name
- `CustomerPhone`: Customer phone number
- `CustomerEmail`: Customer email
- `SubTotal`: Order subtotal before tax and discounts
- `TaxAmount`: Total tax amount
- `DiscountAmount`: Total discount amount
- `TotalAmount`: Final order total
- `PaymentMethod`: Payment method used
- `PaymentStatus`: Payment status (Pending, Paid, Failed, etc.)
- `OrderStatus`: Order status (Pending, Processing, Completed, Cancelled)
- `Notes`: Additional notes
- `OrderDate`: Date when order was created
- `CompletedDate`: Date when order was completed
- `CreatedAt`: Record creation timestamp
- `UpdatedAt`: Record last update timestamp
- `OrderItems`: List of order items

### OrderItem
- `Id`: Unique identifier
- `OrderId`: Reference to parent order
- `ProductId`: Reference to product
- `ProductName`: Product name (copied from product)
- `UnitPrice`: Price per unit
- `Quantity`: Quantity ordered
- `SubTotal`: Item subtotal
- `TaxAmount`: Item tax amount
- `DiscountAmount`: Item discount amount
- `TotalAmount`: Item total amount
- `Notes`: Item-specific notes

## API Endpoints

### Create Order
**POST** `/api/orders`

Creates a new order with items.

**Request Body:**
```json
{
  "customerId": 1,
  "customerName": "John Doe",
  "customerPhone": "+1234567890",
  "customerEmail": "john@example.com",
  "paymentMethod": "Cash",
  "notes": "Special instructions",
  "orderItems": [
    {
      "productId": 1,
      "quantity": 2,
      "notes": "Extra fresh"
    },
    {
      "productId": 3,
      "quantity": 1
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "orderNumber": "ORD-20241201-0001",
    "customerId": 1,
    "customerName": "John Doe",
    "subTotal": 25.00,
    "taxAmount": 2.50,
    "discountAmount": 0.00,
    "totalAmount": 27.50,
    "orderStatus": "Pending",
    "paymentStatus": "Pending",
    "orderDate": "2024-12-01T10:30:00Z",
    "orderItems": [...]
  }
}
```

### Get Order by ID
**GET** `/api/orders/{id}`

Retrieves a specific order by its ID.

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "orderNumber": "ORD-20241201-0001",
    ...
  }
}
```

### Get All Orders
**GET** `/api/orders`

Retrieves all orders with optional filtering and pagination.

**Query Parameters:**
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `orderNumber`: Filter by order number
- `customerName`: Filter by customer name
- `customerPhone`: Filter by customer phone
- `orderStatus`: Filter by order status
- `paymentStatus`: Filter by payment status
- `startDate`: Filter orders from this date (DateTime)
- `endDate`: Filter orders until this date (DateTime)
- `orderStartDate`: Filter orders from this date (DateTimeOffset)
- `orderEndDate`: Filter orders until this date (DateTimeOffset)
- `minTotal`: Minimum order total
- `maxTotal`: Maximum order total

**Examples:**
```
GET /api/orders?pageNumber=1&pageSize=20&orderStatus=Pending&startDate=2024-12-01
GET /api/orders?orderStartDate=2025-08-28T23:59:59.000Z&orderEndDate=2025-08-30T23:59:59.999Z
```

### Update Order
**PATCH** `/api/orders/{id}`

Updates an existing order.

**Request Body:**
```json
{
  "orderStatus": "Completed",
  "paymentStatus": "Paid",
  "notes": "Order completed successfully"
}
```

### Delete Order
**DELETE** `/api/orders/{id}`

Deletes an order.

### Get Orders by Status
**GET** `/api/orders/status/{status}`

Retrieves all orders with a specific status.

**Example:**
```
GET /api/orders/status/Pending
```

### Get Orders by Customer
**GET** `/api/orders/customer/{customerId}`

Retrieves all orders for a specific customer.

**Example:**
```
GET /api/orders/customer/1
```

## Business Logic

The Order API includes the following business logic:

1. **Automatic Order Number Generation**: Orders are automatically assigned a unique order number in the format `ORD-YYYYMMDD-XXXX`
2. **Automatic Total Calculation**: Subtotal, tax, discount, and total amounts are automatically calculated based on product prices and rates
3. **Product Validation**: The system validates that all products in an order exist before creating the order
4. **Default Status Assignment**: New orders are automatically assigned "Pending" status for both order and payment
5. **Timestamp Management**: Creation and update timestamps are automatically managed
6. **Automatic Stock Management**: Stock quantities are automatically reduced when orders are created and restored when orders are cancelled or deleted

## Error Handling

The API returns standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

Common error scenarios:
- Invalid product ID in order items
- Missing required order data
- Order not found for updates/deletes
- No orders found for filters

## Stock Management

### Automatic Stock Reduction
When an order is successfully created, the system automatically reduces the stock quantity of each product by the ordered quantity.

### Stock Restoration
Stock quantities are automatically restored in the following scenarios:
- **Order Cancellation**: When order status is changed to "Cancelled"
- **Order Deletion**: When an order is deleted from the system

### Stock Validation
- The system checks if sufficient stock is available before reducing quantities
- If stock reduction fails, the order is still created but a warning is logged
- Stock restoration always succeeds (adds back to inventory)

### Example Stock Flow
```
Product A: Initial Stock = 100
Order Created: Quantity = 5
→ Stock Reduced: 100 - 5 = 95

Order Cancelled: Quantity = 5
→ Stock Restored: 95 + 5 = 100
```
