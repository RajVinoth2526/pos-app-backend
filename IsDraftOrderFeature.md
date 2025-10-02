# Order Draft Feature Documentation

## Overview
The Order Draft feature allows users to create and manage draft orders before finalizing them. This is useful for:
- Preparing orders offline
- Saving incomplete orders for later completion
- Reviewing orders before submission

## ✅ Implementation Complete

### Database Changes
- ✅ Added `IsDraft` boolean column to Orders table
- ✅ Default value: `false` (orders are final by default)
- ✅ Migration created: `20250920180000_AddIsDraftToOrder`

### Model Updates
- ✅ `Order` model: Added `IsDraft` property
- ✅ `OrderDto` model: Added `IsDraft` property (nullable)
- ✅ `CreateOrderDto` model: Added `IsDraft` property (default: false)
- ✅ `OrderFilterDto` model: Added `IsDraft` filter property

### Business Logic
- ✅ **Draft Orders**: Status automatically set to "Draft", PaymentStatus to "Draft"
- ✅ **Final Orders**: Status set to "Pending", PaymentStatus to "Pending"
- ✅ **Conversion Logic**: Draft → Final automatically updates statuses
- ✅ **Filtering**: Can filter orders by draft status

### API Endpoints

#### New Endpoints Added:

1. **GET /api/orders/drafts**
   - Returns all draft orders
   - Supports pagination and filtering
   - Query parameters: `OrderFilterDto` (except IsDraft is set to true)

2. **GET /api/orders/final**
   - Returns all final (non-draft) orders
   - Supports pagination and filtering
   - Query parameters: `OrderFilterDto` (except IsDraft is set to false)

3. **POST /api/orders/{id}/convert-to-final**
   - Converts a draft order to final order
   - Updates IsDraft to false, OrderStatus to "Pending", PaymentStatus to "Pending"

4. **POST /api/orders/{id}/convert-to-draft**
   - Converts a final order to draft order
   - Updates IsDraft to true, OrderStatus to "Draft", PaymentStatus to "Draft"

#### Updated Endpoints:

1. **POST /api/orders** (Create Order)
   - Now accepts `IsDraft` in request body
   - Automatically sets appropriate statuses based on IsDraft value

2. **PATCH /api/orders/{id}** (Update Order)
   - Now accepts `IsDraft` in request body
   - Automatically handles status transitions when IsDraft changes

3. **GET /api/orders** (Get All Orders)
   - Now supports filtering by IsDraft status
   - Use query parameter: `?IsDraft=true` or `?IsDraft=false`

## Usage Examples

### Creating a Draft Order
```json
POST /api/orders
{
  "CustomerId": 1,
  "CustomerName": "John Doe",
  "CustomerPhone": "123-456-7890",
  "CustomerEmail": "john@example.com",
  "PaymentMethod": "Cash",
  "IsDraft": true,
  "Notes": "Draft order for review",
  "OrderItems": [
    {
      "ProductId": 1,
      "Quantity": 2,
      "Notes": "Special request"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "orderNumber": "ORD-20250920-001",
    "customerId": 1,
    "customerName": "John Doe",
    "isDraft": true,
    "orderStatus": "Draft",
    "paymentStatus": "Draft",
    "subTotal": 20.00,
    "taxAmount": 2.00,
    "totalAmount": 22.00,
    "orderDate": "2025-09-20T10:00:00Z",
    "orderItems": [...]
  }
}
```

### Creating a Final Order
```json
POST /api/orders
{
  "CustomerId": 1,
  "CustomerName": "John Doe",
  "IsDraft": false,
  "OrderItems": [...]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 124,
    "isDraft": false,
    "orderStatus": "Pending",
    "paymentStatus": "Pending",
    ...
  }
}
```

### Filtering Orders by Draft Status
```http
GET /api/orders?IsDraft=true&PageNumber=1&PageSize=10
GET /api/orders?IsDraft=false&PageNumber=1&PageSize=10
GET /api/orders/drafts?PageNumber=1&PageSize=10
GET /api/orders/final?PageNumber=1&PageSize=10
```

### Converting Draft to Final
```http
POST /api/orders/123/convert-to-final
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "isDraft": false,
    "orderStatus": "Pending",
    "paymentStatus": "Pending",
    ...
  }
}
```

### Converting Final to Draft
```http
POST /api/orders/124/convert-to-draft
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 124,
    "isDraft": true,
    "orderStatus": "Draft",
    "paymentStatus": "Draft",
    ...
  }
}
```

## Database Schema

### Orders Table
```sql
CREATE TABLE Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderNumber TEXT NOT NULL,
    CustomerId INTEGER NOT NULL,
    CustomerName TEXT,
    CustomerPhone TEXT,
    CustomerEmail TEXT,
    SubTotal TEXT NOT NULL,
    TaxAmount TEXT NOT NULL,
    DiscountAmount TEXT NOT NULL,
    TotalAmount TEXT NOT NULL,
    PaymentMethod TEXT,
    PaymentStatus TEXT,
    OrderStatus TEXT,
    IsDraft INTEGER NOT NULL DEFAULT 0,  -- NEW COLUMN
    Notes TEXT,
    OrderDate TEXT NOT NULL,
    CompletedDate TEXT,
    CreatedAt TEXT,
    UpdatedAt TEXT
);
```

## Business Rules

1. **Default Behavior**: Orders are final by default (`IsDraft = false`)
2. **Status Mapping**:
   - Draft Orders: `OrderStatus = "Draft"`, `PaymentStatus = "Draft"`
   - Final Orders: `OrderStatus = "Pending"`, `PaymentStatus = "Pending"`
3. **Conversion Rules**:
   - Draft → Final: Statuses change to "Pending"
   - Final → Draft: Statuses change to "Draft"
4. **Filtering**: All existing filters work with draft status filtering

## Migration

The migration adds the `IsDraft` column with a default value of `false` (0), ensuring existing orders remain as final orders.

```sql
ALTER TABLE Orders ADD COLUMN IsDraft INTEGER NOT NULL DEFAULT 0;
```

## Testing

### Test Scenarios
1. ✅ Create draft order
2. ✅ Create final order  
3. ✅ Convert draft to final
4. ✅ Convert final to draft
5. ✅ Filter by draft status
6. ✅ Update order draft status
7. ✅ Get draft orders only
8. ✅ Get final orders only

### API Testing
```bash
# Test draft creation
curl -X POST http://localhost:5172/api/orders \
  -H "Content-Type: application/json" \
  -d '{"CustomerId":1,"IsDraft":true,"OrderItems":[]}'

# Test draft filtering
curl "http://localhost:5172/api/orders?IsDraft=true"

# Test conversion
curl -X POST http://localhost:5172/api/orders/1/convert-to-final
```

## ✅ Feature Complete

The Order Draft feature is fully implemented and ready for use. All endpoints have been updated, the database schema has been migrated, and the business logic handles draft/final order transitions automatically.

