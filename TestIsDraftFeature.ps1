# PowerShell script to test the IsDraft functionality
# Run this script after starting the application

$baseUrl = "http://localhost:5172/api"

Write-Host "Testing IsDraft Order Feature..." -ForegroundColor Green

# Test 1: Create a draft order
Write-Host "`n1. Testing Draft Order Creation..." -ForegroundColor Yellow
$draftOrder = @{
    CustomerId = 1
    CustomerName = "Test Customer"
    CustomerPhone = "123-456-7890"
    CustomerEmail = "test@example.com"
    PaymentMethod = "Cash"
    IsDraft = $true
    Notes = "This is a draft order for testing"
    OrderItems = @(
        @{
            ProductId = 1
            Quantity = 2
            Notes = "Test item"
        }
    )
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $draftOrder -ContentType "application/json"
    Write-Host "✅ Draft order created successfully!" -ForegroundColor Green
    Write-Host "Order ID: $($response.data.id)" -ForegroundColor Cyan
    Write-Host "IsDraft: $($response.data.isDraft)" -ForegroundColor Cyan
    Write-Host "Order Status: $($response.data.orderStatus)" -ForegroundColor Cyan
    Write-Host "Payment Status: $($response.data.paymentStatus)" -ForegroundColor Cyan
    $draftOrderId = $response.data.id
} catch {
    Write-Host "❌ Failed to create draft order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Create a final order
Write-Host "`n2. Testing Final Order Creation..." -ForegroundColor Yellow
$finalOrder = @{
    CustomerId = 1
    CustomerName = "Test Customer 2"
    CustomerPhone = "987-654-3210"
    CustomerEmail = "test2@example.com"
    PaymentMethod = "Credit Card"
    IsDraft = $false
    Notes = "This is a final order for testing"
    OrderItems = @(
        @{
            ProductId = 1
            Quantity = 1
            Notes = "Final test item"
        }
    )
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $finalOrder -ContentType "application/json"
    Write-Host "✅ Final order created successfully!" -ForegroundColor Green
    Write-Host "Order ID: $($response.data.id)" -ForegroundColor Cyan
    Write-Host "IsDraft: $($response.data.isDraft)" -ForegroundColor Cyan
    Write-Host "Order Status: $($response.data.orderStatus)" -ForegroundColor Cyan
    Write-Host "Payment Status: $($response.data.paymentStatus)" -ForegroundColor Cyan
    $finalOrderId = $response.data.id
} catch {
    Write-Host "❌ Failed to create final order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get draft orders
Write-Host "`n3. Testing Draft Orders Retrieval..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders/drafts" -Method GET
    Write-Host "✅ Draft orders retrieved successfully!" -ForegroundColor Green
    Write-Host "Number of draft orders: $($response.data.items.Count)" -ForegroundColor Cyan
    foreach ($order in $response.data.items) {
        Write-Host "  - Order $($order.id): $($order.orderNumber) (IsDraft: $($order.isDraft))" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Failed to retrieve draft orders: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get final orders
Write-Host "`n4. Testing Final Orders Retrieval..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders/final" -Method GET
    Write-Host "✅ Final orders retrieved successfully!" -ForegroundColor Green
    Write-Host "Number of final orders: $($response.data.items.Count)" -ForegroundColor Cyan
    foreach ($order in $response.data.items) {
        Write-Host "  - Order $($order.id): $($order.orderNumber) (IsDraft: $($order.isDraft))" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Failed to retrieve final orders: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Convert draft to final (if we have a draft order)
if ($draftOrderId) {
    Write-Host "`n5. Testing Draft to Final Conversion..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/orders/$draftOrderId/convert-to-final" -Method POST
        Write-Host "✅ Draft order converted to final successfully!" -ForegroundColor Green
        Write-Host "Order ID: $($response.data.id)" -ForegroundColor Cyan
        Write-Host "IsDraft: $($response.data.isDraft)" -ForegroundColor Cyan
        Write-Host "Order Status: $($response.data.orderStatus)" -ForegroundColor Cyan
        Write-Host "Payment Status: $($response.data.paymentStatus)" -ForegroundColor Cyan
    } catch {
        Write-Host "❌ Failed to convert draft to final: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 6: Convert final to draft (if we have a final order)
if ($finalOrderId) {
    Write-Host "`n6. Testing Final to Draft Conversion..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/orders/$finalOrderId/convert-to-draft" -Method POST
        Write-Host "✅ Final order converted to draft successfully!" -ForegroundColor Green
        Write-Host "Order ID: $($response.data.id)" -ForegroundColor Cyan
        Write-Host "IsDraft: $($response.data.isDraft)" -ForegroundColor Cyan
        Write-Host "Order Status: $($response.data.orderStatus)" -ForegroundColor Cyan
        Write-Host "Payment Status: $($response.data.paymentStatus)" -ForegroundColor Cyan
    } catch {
        Write-Host "❌ Failed to convert final to draft: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 7: Filter orders by IsDraft status
Write-Host "`n7. Testing Order Filtering by IsDraft..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders?IsDraft=true" -Method GET
    Write-Host "✅ Draft orders filtered successfully!" -ForegroundColor Green
    Write-Host "Number of draft orders (filtered): $($response.data.items.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to filter draft orders: $($_.Exception.Message)" -ForegroundColor Red
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders?IsDraft=false" -Method GET
    Write-Host "✅ Final orders filtered successfully!" -ForegroundColor Green
    Write-Host "Number of final orders (filtered): $($response.data.items.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to filter final orders: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 IsDraft Feature Testing Complete!" -ForegroundColor Green
Write-Host "Check the results above to verify all functionality is working correctly." -ForegroundColor White

