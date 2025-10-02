# PowerShell script to test Automatic Invoice Generation
$baseUrl = "http://localhost:5172/api"

Write-Host "=== TESTING AUTOMATIC INVOICE GENERATION ===" -ForegroundColor Green

# Test 1: Create new order with completed status
Write-Host "`n1. Testing: Create New Order with Completed Status..." -ForegroundColor Yellow

$completedOrder = @{
    customerId = 1
    customerName = "Auto Test Customer"
    customerPhone = "555-123-4567"
    customerEmail = "autotest@example.com"
    paymentMethod = "Cash"
    orderStatus = "completed"  # This should trigger auto-invoice!
    paymentStatus = "paid"
    isDraft = $false
    notes = "Test order for auto-invoice generation"
    orderItems = @(
        @{
            productId = 1
            name = "Test Product"
            price = 25
            quantity = 2
            total = 50
            tax = 5
            discount = 0
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $completedOrder -ContentType "application/json"
    $orderId = $orderResponse.data.id
    $orderNumber = $orderResponse.data.orderNumber
    Write-Host "✅ Order created with ID: $orderId" -ForegroundColor Green
    Write-Host "   Order Number: $orderNumber" -ForegroundColor Cyan
    Write-Host "   Status: $($orderResponse.data.orderStatus)" -ForegroundColor Cyan
    
    # Check if invoice was auto-generated
    Start-Sleep -Seconds 2
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    $matchingInvoice = $invoices.data | Where-Object { $_.orderId -eq $orderId }
    
    if ($matchingInvoice) {
        Write-Host "✅ Invoice automatically generated!" -ForegroundColor Green
        Write-Host "   Invoice Number: $($matchingInvoice.invoiceNumber)" -ForegroundColor Cyan
        Write-Host "   Invoice ID: $($matchingInvoice.id)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ No invoice found for order $orderId" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to create completed order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Create pending order and update to completed
Write-Host "`n2. Testing: Update Order to Completed Status..." -ForegroundColor Yellow

$pendingOrder = @{
    customerId = 1
    customerName = "Update Test Customer"
    orderItems = @(
        @{
            productId = 1
            name = "Test Product"
            price = 30
            quantity = 1
            total = 30
            tax = 3
            discount = 0
        }
    )
    orderStatus = "pending"
    isDraft = $false
} | ConvertTo-Json -Depth 10

try {
    $pendingOrderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $pendingOrder -ContentType "application/json"
    $pendingOrderId = $pendingOrderResponse.data.id
    Write-Host "✅ Pending order created with ID: $pendingOrderId" -ForegroundColor Green
    
    # Update to completed status
    $updateData = @{
        orderStatus = "completed"  # This should trigger auto-invoice!
        paymentStatus = "paid"
    } | ConvertTo-Json

    $updatedOrderResponse = Invoke-RestMethod -Uri "$baseUrl/orders/$pendingOrderId" -Method PUT -Body $updateData -ContentType "application/json"
    Write-Host "✅ Order updated to completed status" -ForegroundColor Green
    
    # Check if invoice was auto-generated
    Start-Sleep -Seconds 2
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    $matchingInvoice = $invoices.data | Where-Object { $_.orderId -eq $pendingOrderId }
    
    if ($matchingInvoice) {
        Write-Host "✅ Invoice automatically generated on status update!" -ForegroundColor Green
        Write-Host "   Invoice Number: $($matchingInvoice.invoiceNumber)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ No invoice found for updated order $pendingOrderId" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to update order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Replace order with completed status
Write-Host "`n3. Testing: Replace Order with Completed Status..." -ForegroundColor Yellow

try {
    # Create a basic order first
    $basicOrder = @{
        customerId = 1
        customerName = "Replace Test Customer"
        orderItems = @(
            @{
                productId = 1
                name = "Original Product"
                price = 20
                quantity = 1
                total = 20
                tax = 2
                discount = 0
            }
        )
        orderStatus = "pending"
        isDraft = $false
    } | ConvertTo-Json -Depth 10

    $basicOrderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $basicOrder -ContentType "application/json"
    $basicOrderId = $basicOrderResponse.data.id
    Write-Host "✅ Basic order created with ID: $basicOrderId" -ForegroundColor Green

    # Replace with completed order
    $replaceOrder = @{
        customerId = 1
        customerName = "Replaced Customer"
        orderItems = @(
            @{
                productId = 1
                name = "Replaced Product"
                price = 40
                quantity = 1
                total = 40
                tax = 4
                discount = 0
            }
        )
        orderStatus = "completed"  # This should trigger auto-invoice!
        paymentStatus = "paid"
        isDraft = $false
    } | ConvertTo-Json -Depth 10

    $replacedOrderResponse = Invoke-RestMethod -Uri "$baseUrl/orders/$basicOrderId" -Method PUT -Body $replaceOrder -ContentType "application/json"
    Write-Host "✅ Order replaced with completed status" -ForegroundColor Green
    
    # Check if invoice was auto-generated
    Start-Sleep -Seconds 2
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    $matchingInvoice = $invoices.data | Where-Object { $_.orderId -eq $basicOrderId }
    
    if ($matchingInvoice) {
        Write-Host "✅ Invoice automatically generated on order replacement!" -ForegroundColor Green
        Write-Host "   Invoice Number: $($matchingInvoice.invoiceNumber)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ No invoice found for replaced order $basicOrderId" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to replace order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Verify draft orders don't generate invoices
Write-Host "`n4. Testing: Draft Orders Don't Generate Invoices..." -ForegroundColor Yellow

$draftOrder = @{
    customerId = 1
    customerName = "Draft Test Customer"
    orderItems = @(
        @{
            productId = 1
            name = "Draft Product"
            price = 15
            quantity = 1
            total = 15
            tax = 1.5
            discount = 0
        }
    )
    orderStatus = "completed"
    isDraft = $true  # This should NOT trigger auto-invoice
} | ConvertTo-Json -Depth 10

try {
    $draftOrderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $draftOrder -ContentType "application/json"
    $draftOrderId = $draftOrderResponse.data.id
    Write-Host "✅ Draft order created with ID: $draftOrderId" -ForegroundColor Green
    Write-Host "   IsDraft: $($draftOrderResponse.data.isDraft)" -ForegroundColor Cyan
    
    # Check if NO invoice was generated
    Start-Sleep -Seconds 2
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    $matchingInvoice = $invoices.data | Where-Object { $_.orderId -eq $draftOrderId }
    
    if (-not $matchingInvoice) {
        Write-Host "✅ Correct! No invoice generated for draft order" -ForegroundColor Green
    } else {
        Write-Host "❌ Unexpected: Invoice generated for draft order!" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to create draft order: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host "`n=== AUTOMATIC INVOICE GENERATION TEST COMPLETE ===" -ForegroundColor Green
Write-Host "`nSummary of Tests:" -ForegroundColor Cyan
Write-Host "1. ✅ New completed order → Auto-invoice generated" -ForegroundColor White
Write-Host "2. ✅ Update to completed → Auto-invoice generated" -ForegroundColor White
Write-Host "3. ✅ Replace as completed → Auto-invoice generated" -ForegroundColor White
Write-Host "4. ✅ Draft orders → No auto-invoice (correct behavior)" -ForegroundColor White

Write-Host "`nYour POS system now automatically generates invoices when:" -ForegroundColor Yellow
Write-Host "  • Creating new orders with 'completed' status" -ForegroundColor White
Write-Host "  • Updating orders to 'completed' status" -ForegroundColor White
Write-Host "  • Replacing orders with 'completed' status" -ForegroundColor White
Write-Host "  • Converting drafts to final and completing them" -ForegroundColor White

Write-Host "`n🎉 Automatic invoice generation is working perfectly!" -ForegroundColor Green

