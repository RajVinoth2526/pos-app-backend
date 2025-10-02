# Invoice System Documentation

## Overview
The Invoice System provides complete invoice generation, management, and printing functionality for your POS system. It automatically creates professional invoices from completed orders and supports both HTML and PDF formats.

## ✅ Features Implemented

### 🎯 Core Functionality
- ✅ **Automatic Invoice Generation** from completed orders
- ✅ **Professional HTML Templates** with responsive design
- ✅ **Print-Ready Formatting** with print CSS
- ✅ **Invoice Management** (CRUD operations)
- ✅ **Status Tracking** (Generated, Sent, Paid, Cancelled)
- ✅ **Customizable Templates** with company branding

### 🎨 Template Features
- ✅ **Professional Design** with modern styling
- ✅ **Responsive Layout** works on all devices
- ✅ **Print Optimization** with print-specific CSS
- ✅ **Company Branding** (logo, colors, contact info)
- ✅ **Tax Breakdown** display
- ✅ **Terms & Conditions** section
- ✅ **Automatic Numbering** (INV-YYYYMMDD-001 format)

## 📋 API Endpoints

### Invoice Generation
```http
POST /api/invoices/generate/{orderId}     # Generate invoice from order
POST /api/orders/{id}/generate-invoice    # Generate invoice from order (alternative)
```

### Invoice Management
```http
GET    /api/invoices                      # Get all invoices
GET    /api/invoices/{id}                 # Get invoice by ID
GET    /api/invoices/order/{orderId}      # Get invoice by order ID
PUT    /api/invoices/{id}/status          # Update invoice status
DELETE /api/invoices/{id}                 # Delete invoice
```

### Invoice Printing & Display
```http
GET  /api/invoices/{id}/html              # Get invoice as HTML
POST /api/invoices/{id}/print             # Print invoice (HTML/PDF)
```

## 🚀 Quick Start Guide

### 1. Generate Invoice from Order
```bash
# Generate invoice for order ID 123
curl -X POST "http://localhost:5172/api/orders/123/generate-invoice" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "invoiceNumber": "INV-20250920-001",
    "orderId": 123,
    "invoiceDate": "2025-09-20T10:30:00Z",
    "dueDate": "2025-10-20T10:30:00Z",
    "status": "Generated",
    "subTotal": 100.00,
    "taxAmount": 10.00,
    "totalAmount": 110.00,
    "invoiceItems": [...]
  }
}
```

### 2. View Invoice HTML
```bash
# Get invoice as HTML for display/printing
curl "http://localhost:5172/api/invoices/1/html"
```

### 3. Print Invoice
```bash
# Print invoice with print controls
curl -X POST "http://localhost:5172/api/invoices/1/print" \
  -H "Content-Type: application/json" \
  -d '{"format": "HTML", "includeLogo": true}'
```

## 🎨 Invoice Template Customization

### Default Template Settings
```csharp
CompanyName = "Your POS Store"
CompanyAddress = "123 Main Street, City, State 12345"
CompanyPhone = "(555) 123-4567"
CompanyEmail = "info@yourposstore.com"
TaxNumber = "TAX-123456789"
Website = "www.yourposstore.com"
Currency = "USD"
FooterText = "Thank you for your business!"
TermsAndConditions = "Payment is due within 30 days..."
```

### Customizing the Template
To customize the invoice template, modify the `GetDefaultTemplate()` method in `InvoiceService.cs`:

```csharp
private InvoiceTemplate GetDefaultTemplate()
{
    return new InvoiceTemplate
    {
        CompanyName = "Your Company Name",
        CompanyAddress = "Your Address",
        CompanyPhone = "Your Phone",
        CompanyEmail = "Your Email",
        TaxNumber = "Your Tax Number",
        Website = "Your Website",
        Currency = "USD",
        FooterText = "Your Footer Message",
        TermsAndConditions = "Your Terms and Conditions"
    };
}
```

## 🖨️ Printing Options

### 1. Browser Print (Recommended)
- Access: `GET /api/invoices/{id}/html`
- Click browser's print button (Ctrl+P)
- Select printer and settings
- Professional formatting included

### 2. Print with Controls
- Access: `POST /api/invoices/{id}/print`
- Includes print buttons and controls
- Optimized for printing
- Can close window after printing

### 3. Print CSS Features
```css
@media print {
    body { background-color: white; }
    .no-print { display: none; }
    @page { margin: 0.5in; }
    /* Professional print formatting */
}
```

## 📊 Invoice Data Structure

### Invoice Fields
- **Invoice Number**: Auto-generated (INV-YYYYMMDD-001)
- **Order Reference**: Links to original order
- **Dates**: Invoice date, due date
- **Status**: Generated, Sent, Paid, Cancelled
- **Totals**: Subtotal, tax, discount, total
- **Customer Info**: From order data
- **Items**: Detailed line items

### Invoice Items
- **Product Details**: Name, description, ID
- **Pricing**: Unit price, quantity, totals
- **Taxes**: Individual item tax amounts
- **Discounts**: Item-level discounts

## 🔧 Database Schema

### Invoices Table
```sql
CREATE TABLE Invoices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceNumber TEXT NOT NULL UNIQUE,
    OrderId INTEGER NOT NULL,
    InvoiceDate TEXT NOT NULL,
    DueDate TEXT,
    Status TEXT NOT NULL DEFAULT 'Generated',
    SubTotal TEXT NOT NULL,
    TaxAmount TEXT NOT NULL,
    DiscountAmount TEXT NOT NULL,
    TotalAmount TEXT NOT NULL,
    Notes TEXT,
    TermsAndConditions TEXT,
    CreatedAt TEXT,
    UpdatedAt TEXT,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);
```

### InvoiceItems Table
```sql
CREATE TABLE InvoiceItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    ProductName TEXT NOT NULL,
    ProductDescription TEXT,
    UnitPrice TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    SubTotal TEXT NOT NULL,
    TaxAmount TEXT NOT NULL,
    DiscountAmount TEXT NOT NULL,
    TotalAmount TEXT NOT NULL,
    Notes TEXT,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
);
```

## 📱 Frontend Integration

### JavaScript Example
```javascript
// Generate invoice
async function generateInvoice(orderId) {
    const response = await fetch(`/api/orders/${orderId}/generate-invoice`, {
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    });
    const result = await response.json();
    return result.data;
}

// Print invoice
function printInvoice(invoiceId) {
    window.open(`/api/invoices/${invoiceId}/html`, '_blank');
}

// Print with controls
function printWithControls(invoiceId) {
    window.open(`/api/invoices/${invoiceId}/print`, '_blank', 'width=800,height=600');
}
```

### React Component Example
```jsx
function InvoiceButton({ orderId }) {
    const handleGenerateInvoice = async () => {
        try {
            const invoice = await generateInvoice(orderId);
            // Open invoice in new window for printing
            window.open(`/api/invoices/${invoice.id}/html`, '_blank');
        } catch (error) {
            console.error('Error generating invoice:', error);
        }
    };

    return (
        <button onClick={handleGenerateInvoice}>
            🧾 Generate & Print Invoice
        </button>
    );
}
```

## 🎯 Use Cases

### 1. Order Completion Flow
```bash
# 1. Complete order
PUT /api/orders/123
{
  "orderStatus": "completed",
  "paymentStatus": "paid"
}

# 2. Generate invoice
POST /api/orders/123/generate-invoice

# 3. Print invoice
GET /api/invoices/1/html
```

### 2. Batch Invoice Generation
```bash
# Generate invoices for multiple completed orders
for orderId in 123 124 125; do
  curl -X POST "http://localhost:5172/api/orders/$orderId/generate-invoice"
done
```

### 3. Invoice Management
```bash
# Get all invoices
GET /api/invoices

# Update invoice status
PUT /api/invoices/1/status
"Paid"

# Delete invoice
DELETE /api/invoices/1
```

## 🔒 Security Considerations

- **Authentication Required**: All endpoints require valid JWT token
- **Order Validation**: Only existing orders can generate invoices
- **Status Tracking**: Invoice status changes are logged
- **Data Integrity**: Foreign key constraints ensure data consistency

## 📈 Performance Features

- **Efficient Queries**: Optimized database queries with includes
- **Caching Ready**: Template caching can be easily added
- **Async Operations**: All operations are async for better performance
- **Minimal Memory**: HTML generation uses StringBuilder for efficiency

## 🎉 Ready to Use!

Your POS system now has complete invoice functionality:

1. **✅ Generate invoices** from completed orders
2. **✅ Professional templates** with your branding
3. **✅ Print-ready formatting** for any printer
4. **✅ Complete management** of invoice lifecycle
5. **✅ API endpoints** for frontend integration

**Start generating invoices immediately after order completion!** 🚀

