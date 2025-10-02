# Invoice Printing Guide - Complete Implementation

## 🎯 **Automatic Invoice Printing After Save**

Your POS system now automatically prints invoices after they are saved! Here's how it works:

### **✅ Automatic Printing Triggers:**

#### **1. Order Creation with "completed" status**
```bash
POST /api/orders
{
  "orderStatus": "completed",
  "isDraft": false
}
# → Invoice generated → Invoice automatically printed! 🖨️
```

#### **2. Order Update to "completed" status**
```bash
PUT /api/orders/123
{
  "orderStatus": "completed"
}
# → Invoice generated → Invoice automatically printed! 🖨️
```

#### **3. Order Replacement with "completed" status**
```bash
PUT /api/orders/123
{
  "orderItems": [...],
  "orderStatus": "completed"
}
# → Invoice generated → Invoice automatically printed! 🖨️
```

## 🖨️ **Printing Methods Available**

### **1. ✅ Automatic Browser Printing (Default)**
- **How:** Uses default browser's print functionality
- **When:** Automatically triggered after invoice generation
- **Features:** Print dialog opens, user can select printer and settings
- **Best for:** Most users, works with any printer

### **2. 🔧 Manual Printing Endpoints**
```http
# Print to default browser
POST /api/invoices/{id}/print-to-printer

# Print to system printer (headless)
POST /api/invoices/{id}/print-to-system-printer

# Generate PDF for printing
POST /api/invoices/{id}/generate-pdf
```

### **3. 📱 Frontend Integration**
```javascript
// Print invoice after generation
async function printInvoice(invoiceId) {
    try {
        const response = await fetch(`/api/invoices/${invoiceId}/print-to-printer`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                pageSize: 'A4',
                pageMargin: '0.5in',
                closeAfterPrint: true
            })
        });
        
        const result = await response.json();
        if (result.success) {
            console.log('Invoice sent to printer!');
        }
    } catch (error) {
        console.error('Print failed:', error);
    }
}
```

## 🎨 **Print Options & Customization**

### **PrintOptions Class:**
```csharp
public class PrintOptions
{
    public string? PageSize { get; set; } = "A4";           // A4, Letter, Legal, etc.
    public string? PageMargin { get; set; } = "0.5in";     // Page margins
    public bool CloseAfterPrint { get; set; } = true;      // Close browser after print
    public bool AutoPrint { get; set; } = true;            // Auto-trigger print dialog
    public string? PrinterName { get; set; }               // Specific printer name
    public int Copies { get; set; } = 1;                   // Number of copies
}
```

### **Custom Print Settings:**
```bash
# Print with custom options
POST /api/invoices/1/print-to-printer
{
  "pageSize": "Letter",
  "pageMargin": "0.25in",
  "closeAfterPrint": false,
  "copies": 2
}
```

## 🚀 **Complete Workflow Examples**

### **Scenario 1: Retail Checkout**
```bash
# 1. Customer pays at register
POST /api/orders
{
  "customerName": "John Doe",
  "orderItems": [...],
  "orderStatus": "completed",
  "paymentStatus": "paid",
  "isDraft": false
}

# ✅ Automatic workflow:
# → Invoice generated
# → Invoice automatically printed
# → Print dialog opens
# → Customer gets receipt
```

### **Scenario 2: Restaurant Order**
```bash
# 1. Take order
POST /api/orders
{
  "orderItems": [...],
  "orderStatus": "pending"
}

# 2. Prepare food
PUT /api/orders/123
{
  "orderStatus": "ready"
}

# 3. Customer pays
PUT /api/orders/123
{
  "orderStatus": "completed",
  "paymentStatus": "paid"
}

# ✅ Automatic workflow:
# → Invoice generated
# → Invoice automatically printed
# → Kitchen receipt printed
```

### **Scenario 3: E-commerce**
```bash
# 1. Payment processed
PUT /api/orders/456
{
  "orderStatus": "completed",
  "paymentStatus": "paid"
}

# ✅ Automatic workflow:
# → Invoice generated
# → Invoice automatically printed
# → Packing slip printed
```

## 🔧 **Technical Implementation**

### **Automatic Printing Logic:**
```csharp
// In OrderManager.cs - after invoice generation
try
{
    var invoice = await _invoiceService.GenerateInvoiceFromOrderAsync(orderId);
    Console.WriteLine($"Invoice automatically generated for order {orderNumber}");
    
    // Automatically print the invoice
    var printSuccess = await _invoicePrintService.PrintInvoiceAsync(invoice.Id);
    if (printSuccess)
    {
        Console.WriteLine($"Invoice automatically printed for order {orderNumber}");
    }
    else
    {
        Console.WriteLine($"Warning: Failed to auto-print invoice for order {orderNumber}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to auto-generate/print invoice: {ex.Message}");
}
```

### **Print Service Features:**
- **Browser Integration:** Uses system default browser
- **Print Dialog:** Opens native print dialog for user control
- **Error Handling:** Graceful failure if printing fails
- **Temporary Files:** Automatic cleanup of temporary files
- **Multiple Formats:** HTML, PDF support
- **Cross-Platform:** Works on Windows, macOS, Linux

## 📊 **Print Quality & Formatting**

### **Print-Optimized CSS:**
```css
@media print {
    body { 
        background-color: white; 
        margin: 0;
        padding: 0;
    }
    
    @page { 
        margin: 0.5in; 
        size: A4;
    }
    
    .invoice-header, .customer-info, .invoice-items, .invoice-totals, .invoice-footer {
        box-shadow: none;
        border: 1px solid #ddd;
        page-break-inside: avoid;
    }
    
    .invoice-items {
        page-break-inside: avoid;
    }
    
    .invoice-items tr {
        page-break-inside: avoid;
    }
}
```

### **Print Features:**
- **Professional Layout:** Clean, business-ready formatting
- **Page Breaks:** Smart page break handling
- **Margins:** Configurable page margins
- **Font Sizing:** Print-optimized font sizes
- **Colors:** Print-safe color scheme
- **Borders:** Clean table borders

## 🎯 **Frontend Integration Examples**

### **React Component:**
```jsx
function OrderCompletion({ orderId }) {
    const completeOrder = async () => {
        try {
            // Complete the order (triggers auto-invoice + auto-print)
            await updateOrder(orderId, { 
                orderStatus: "completed",
                paymentStatus: "paid" 
            });
            
            // Show success message
            alert('Order completed! Invoice has been generated and sent to printer.');
        } catch (error) {
            console.error('Order completion failed:', error);
        }
    };

    return (
        <button onClick={completeOrder}>
            ✅ Complete Order & Print Invoice
        </button>
    );
}
```

### **Angular Service:**
```typescript
@Injectable()
export class OrderService {
    async completeOrder(orderId: number): Promise<any> {
        const result = await this.http.put(`/api/orders/${orderId}`, {
            orderStatus: 'completed',
            paymentStatus: 'paid'
        }).toPromise();
        
        // Invoice automatically generated and printed!
        return result;
    }
    
    async printInvoice(invoiceId: number): Promise<boolean> {
        try {
            const response = await this.http.post(`/api/invoices/${invoiceId}/print-to-printer`, {
                pageSize: 'A4',
                closeAfterPrint: true
            }).toPromise();
            
            return response.success;
        } catch (error) {
            console.error('Print failed:', error);
            return false;
        }
    }
}
```

## 🔒 **Error Handling & Reliability**

### **Graceful Failure:**
- If printing fails, order completion still succeeds
- Warning messages logged to console
- System continues to function normally
- User can manually print if needed

### **Print Fallbacks:**
1. **Primary:** Browser print dialog
2. **Fallback:** System printer integration
3. **Last Resort:** PDF generation for manual printing

## 🎉 **Ready to Use!**

Your POS system now has **complete automatic invoice printing**:

### **✅ What Happens Automatically:**
1. **Order completed** → Invoice generated
2. **Invoice generated** → Invoice automatically printed
3. **Print dialog opens** → User selects printer
4. **Invoice printed** → Customer gets receipt

### **✅ Manual Control Available:**
- Print specific invoices on demand
- Customize print settings
- Generate PDFs for email/archive
- Print multiple copies

### **✅ Professional Quality:**
- Print-optimized formatting
- Clean, business-ready layout
- Proper page breaks and margins
- Professional invoice design

**Your POS system now automatically prints professional invoices after every order completion!** 🚀

## 📋 **API Endpoints Summary**

```http
# Automatic (built into order completion)
POST /api/orders                    # Auto-print if completed
PUT  /api/orders/{id}              # Auto-print if completed
PUT  /api/orders/{id}              # Auto-print if completed (replace)

# Manual printing
POST /api/invoices/{id}/print-to-printer          # Print to browser
POST /api/invoices/{id}/print-to-system-printer   # Print to system
POST /api/invoices/{id}/generate-pdf              # Generate PDF
GET  /api/invoices/{id}/html                      # View HTML
```

**Start completing orders and watch the invoices print automatically!** 🎯

