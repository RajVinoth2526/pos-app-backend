# Invoice Generation Triggers - When Invoices Are Created

## 🎯 **Current Invoice Generation Methods**

### **1. ✅ AUTOMATIC Generation (NEW!)**
**When:** Order status changes to "completed"
**How:** Automatically triggered in the system

```bash
# When you update an order to completed status:
PUT /api/orders/123
{
  "orderStatus": "completed",
  "paymentStatus": "paid"
}

# ✅ Invoice is automatically generated!
```

**Conditions for Auto-Generation:**
- ✅ Order status changes to "completed"
- ✅ Order is NOT a draft (IsDraft = false)
- ✅ Order was not already completed before
- ✅ System automatically calls invoice generation

### **2. 🔧 MANUAL Generation**
**When:** You explicitly request it
**How:** Call the API endpoint

```bash
# Generate invoice manually for any order:
POST /api/orders/123/generate-invoice
# OR
POST /api/invoices/generate/123
```

### **3. 📱 FRONTEND Integration**
**When:** User clicks "Generate Invoice" button
**How:** Frontend calls the API

```javascript
// Frontend button click
function generateInvoice(orderId) {
    fetch(`/api/orders/${orderId}/generate-invoice`, {
        method: 'POST',
        headers: { 'Authorization': 'Bearer ' + token }
    })
    .then(response => response.json())
    .then(data => {
        // Invoice generated successfully
        console.log('Invoice:', data.data.invoiceNumber);
    });
}
```

## 🚀 **Complete Workflow Examples**

### **Scenario 1: Normal Order Completion**
```bash
# 1. Customer places order
POST /api/orders
{
  "customerName": "John Doe",
  "orderItems": [...],
  "orderStatus": "pending"
}

# 2. Order is processed and completed
PUT /api/orders/123
{
  "orderStatus": "completed",
  "paymentStatus": "paid"
}

# ✅ Invoice automatically generated!
# Invoice Number: INV-20250920-001
```

### **Scenario 2: Draft to Final Order**
```bash
# 1. Create draft order
POST /api/orders
{
  "orderItems": [...],
  "isDraft": true,
  "orderStatus": "draft"
}

# 2. Convert draft to final order
POST /api/orders/123/convert-to-final
# Order status becomes "pending"

# 3. Complete the order
PUT /api/orders/123
{
  "orderStatus": "completed"
}

# ✅ Invoice automatically generated!
```

### **Scenario 3: Manual Invoice Generation**
```bash
# Generate invoice for any order (even if not completed)
POST /api/orders/123/generate-invoice

# ✅ Invoice generated manually
```

## 📊 **Invoice Generation Logic**

### **Automatic Generation Rules:**
```csharp
// In OrderManager.UpdateOrderAsync()
if (updatedOrder != null && 
    !string.IsNullOrEmpty(dto.OrderStatus) && 
    dto.OrderStatus.ToLower() == "completed" && 
    currentOrder.OrderStatus?.ToLower() != "completed" &&
    !updatedOrder.IsDraft) // Only for final orders
{
    // Auto-generate invoice
    await _invoiceService.GenerateInvoiceFromOrderAsync(id);
}
```

### **When Invoices Are NOT Generated:**
- ❌ Order is a draft (IsDraft = true)
- ❌ Order was already completed before
- ❌ Order status is not "completed"
- ❌ Order doesn't exist

## 🎯 **Real-World Usage Scenarios**

### **Retail Store Checkout:**
1. **Customer pays** → Order status = "completed"
2. **✅ Invoice automatically generated**
3. **Print invoice** for customer receipt
4. **Customer gets professional invoice**

### **Restaurant Order:**
1. **Order taken** → Order status = "pending"
2. **Food prepared** → Order status = "ready"
3. **Customer pays** → Order status = "completed"
4. **✅ Invoice automatically generated**
5. **Print receipt/invoice**

### **E-commerce:**
1. **Order placed** → Order status = "pending"
2. **Payment processed** → Order status = "completed"
3. **✅ Invoice automatically generated**
4. **Email invoice to customer**

## 🔧 **Customization Options**

### **Change Auto-Generation Trigger:**
```csharp
// In OrderManager.cs - modify the condition
if (dto.OrderStatus.ToLower() == "paid") // Instead of "completed"
{
    // Generate invoice when payment is confirmed
}
```

### **Generate for Different Statuses:**
```csharp
// Generate invoice when order is "shipped"
if (dto.OrderStatus.ToLower() == "shipped")
{
    // Generate shipping invoice
}
```

### **Conditional Generation:**
```csharp
// Only generate invoices for orders over $100
if (updatedOrder.TotalAmount >= 100)
{
    await _invoiceService.GenerateInvoiceFromOrderAsync(id);
}
```

## 📱 **Frontend Integration Examples**

### **React Component:**
```jsx
function OrderCompletion({ orderId }) {
    const completeOrder = async () => {
        // Complete the order
        await updateOrder(orderId, { 
            orderStatus: "completed",
            paymentStatus: "paid" 
        });
        
        // Invoice is automatically generated!
        // You can optionally show a success message
        alert('Order completed! Invoice has been generated.');
    };

    return (
        <button onClick={completeOrder}>
            ✅ Complete Order & Generate Invoice
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
        
        // Invoice automatically generated!
        return result;
    }
}
```

## 🎉 **Summary: When Invoices Are Generated**

### **✅ AUTOMATIC (Recommended):**
- **When:** Order status changes to "completed"
- **Trigger:** System automatically detects status change
- **Benefit:** No manual intervention needed
- **Use Case:** Normal business workflow

### **🔧 MANUAL:**
- **When:** You explicitly call the API
- **Trigger:** Manual API call or button click
- **Benefit:** Full control over when invoices are created
- **Use Case:** Special circumstances or batch processing

### **📱 FRONTEND:**
- **When:** User clicks "Generate Invoice" button
- **Trigger:** User interaction in the UI
- **Benefit:** User-initiated invoice generation
- **Use Case:** On-demand invoice creation

**Your POS system now automatically generates professional invoices whenever orders are completed!** 🚀

