# Date Filtering Examples

## Problem Solved
The issue was that the frontend sends `DateTimeOffset` values (like `8/28/2025 11:59:59 PM +00:00`) but the database stores `DateTime` values (like `2025-08-31 12:17:33.1765296`).

## Solution
The `OrderService` now properly converts `DateTimeOffset` to `DateTime` using `.DateTime` property before comparing with database values.

## Frontend Date Format
```javascript
// Frontend sends DateTimeOffset
{
  orderStartDate: "8/28/2025 11:59:59 PM +00:00",
  orderEndDate: "8/30/2025 11:59:59 PM +00:00"
}
```

## Database Date Format
```sql
-- Database stores DateTime
OrderDate: 2025-08-31 12:17:33.1765296
```

## API Call Examples

### 1. Using DateTimeOffset (Frontend Format)
```bash
curl 'https://localhost:44376/api/orders?orderStartDate=2025-08-28T23:59:59.000Z&orderEndDate=2025-08-30T23:59:59.999Z'
```

### 2. Using DateTime (Simple Format)
```bash
curl 'https://localhost:44376/api/orders?startDate=2025-08-28&endDate=2025-08-30'
```

### 3. Combined with Other Filters
```bash
curl 'https://localhost:44376/api/orders?pageNumber=1&pageSize=10&orderStatus=Pending&orderStartDate=2025-08-28T00:00:00.000Z&orderEndDate=2025-08-30T23:59:59.999Z'
```

## How It Works
```csharp
// In OrderService.cs
if (filters.OrderStartDate.HasValue)
    query = query.Where(o => o.OrderDate >= filters.OrderStartDate.Value.DateTime);

if (filters.OrderEndDate.HasValue)
    query = query.Where(o => o.OrderDate <= filters.OrderEndDate.Value.DateTime);
```

The `.DateTime` property extracts the `DateTime` component from the `DateTimeOffset`, making the comparison work correctly with the database `DateTime` field.

## Supported Date Formats
- **DateTimeOffset**: `2025-08-28T23:59:59.000Z` (ISO 8601 with timezone)
- **DateTime**: `2025-08-28` (Simple date format)
- **Both formats are supported** for maximum flexibility






