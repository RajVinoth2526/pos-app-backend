# Quick test to verify Invoice tables and auto-generation
$baseUrl = "http://localhost:5172/api"

Write-Host "=== QUICK INVOICE SYSTEM TEST ===" -ForegroundColor Green

# Test 1: Check if Invoice endpoints are working
Write-Host "`n1. Testing Invoice API endpoints..." -ForegroundColor Yellow

try {
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    Write-Host "✅ Invoice API is working! Found $($invoices.data.Count) existing invoices." -ForegroundColor Green
} catch {
    Write-Host "❌ Invoice API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Create a completed order to test auto-invoice generation
Write-Host "`n2. Testing automatic invoice generation..." -ForegroundColor Yellow

$completedOrder = @{
    customerId = 1
    customerName = "Test Customer"
    customerPhone = "555-123-4567"
    customerEmail = "test@example.com"
    paymentMethod = "Cash"
    orderStatus = "completed"  # This should trigger auto-invoice!
    paymentStatus = "paid"
    isDraft = $false
    notes = "Test order for auto-invoice"
    orderItems = @(
        @{
            productId = 1
            name = "Test Product"
            price = 25
            quantity = 1
            total = 25
            tax = 2.5
            discount = 0
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $completedOrder -ContentType "application/json"
    $orderId = $orderResponse.data.id
    $orderNumber = $orderResponse.data.orderNumber
    Write-Host "✅ Completed order created with ID: $orderId" -ForegroundColor Green
    Write-Host "   Order Number: $orderNumber" -ForegroundColor Cyan
    Write-Host "   Status: $($orderResponse.data.orderStatus)" -ForegroundColor Cyan
    
    # Wait a moment for auto-invoice generation
    Start-Sleep -Seconds 3
    
    # Check if invoice was auto-generated
    $invoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    $matchingInvoice = $invoices.data | Where-Object { $_.orderId -eq $orderId }
    
    if ($matchingInvoice) {
        Write-Host "✅ AUTOMATIC INVOICE GENERATION WORKING!" -ForegroundColor Green
        Write-Host "   Invoice Number: $($matchingInvoice.invoiceNumber)" -ForegroundColor Cyan
        Write-Host "   Invoice ID: $($matchingInvoice.id)" -ForegroundColor Cyan
        Write-Host "   Total Amount: $($matchingInvoice.totalAmount)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ No invoice found for order $orderId" -ForegroundColor Red
        Write-Host "   This might indicate auto-generation is not working" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Failed to create completed order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Try to get invoice HTML
Write-Host "`n3. Testing invoice HTML generation..." -ForegroundColor Yellow

if ($matchingInvoice) {
    try {
        $htmlResponse = Invoke-WebRequest -Uri "$baseUrl/invoices/$($matchingInvoice.id)/html" -Method GET
        Write-Host "✅ Invoice HTML generated successfully!" -ForegroundColor Green
        Write-Host "   HTML size: $($htmlResponse.Content.Length) characters" -ForegroundColor Cyan
        
        # Save HTML to file
        $htmlFile = "TestInvoice_$($matchingInvoice.invoiceNumber).html"
        $htmlResponse.Content | Out-File -FilePath $htmlFile -Encoding UTF8
        Write-Host "   HTML saved as: $htmlFile" -ForegroundColor Cyan
        
        # Open in browser
        if (Test-Path $htmlFile) {
            Start-Process $htmlFile
            Write-Host "   Invoice opened in browser!" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "❌ Failed to generate HTML: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Green
Write-Host "`nIf you see the invoice in your browser, the system is working perfectly!" -ForegroundColor Yellow
Write-Host "The automatic invoice generation should work for:" -ForegroundColor Cyan
Write-Host "  • New orders with 'completed' status" -ForegroundColor White
Write-Host "  • Order updates to 'completed' status" -ForegroundColor White
Write-Host "  • Order replacements with 'completed' status" -ForegroundColor White
