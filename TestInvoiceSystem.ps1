# PowerShell script to test the Invoice System
$baseUrl = "http://localhost:5172/api"

Write-Host "=== INVOICE SYSTEM TESTING ===" -ForegroundColor Green

# Test 1: Create a test order first
Write-Host "`n1. Creating Test Order..." -ForegroundColor Yellow

$testOrder = @{
    customerId = 1
    customerName = "Test Customer"
    customerPhone = "123-456-7890"
    customerEmail = "test@example.com"
    paymentMethod = "Cash"
    orderStatus = "completed"
    paymentStatus = "paid"
    isDraft = $false
    notes = "Test order for invoice generation"
    orderItems = @(
        @{
            productId = 1
            name = "Test Product"
            price = 50
            quantity = 2
            total = 100
            tax = 10
            discount = 0
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/orders" -Method POST -Body $testOrder -ContentType "application/json"
    $orderId = $orderResponse.data.id
    Write-Host "✅ Test order created with ID: $orderId" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create test order: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Generate invoice from order
Write-Host "`n2. Generating Invoice from Order..." -ForegroundColor Yellow

try {
    $invoiceResponse = Invoke-RestMethod -Uri "$baseUrl/orders/$orderId/generate-invoice" -Method POST -ContentType "application/json"
    $invoiceId = $invoiceResponse.data.id
    $invoiceNumber = $invoiceResponse.data.invoiceNumber
    Write-Host "✅ Invoice generated successfully!" -ForegroundColor Green
    Write-Host "   Invoice ID: $invoiceId" -ForegroundColor Cyan
    Write-Host "   Invoice Number: $invoiceNumber" -ForegroundColor Cyan
    Write-Host "   Order ID: $($invoiceResponse.data.orderId)" -ForegroundColor Cyan
    Write-Host "   Total Amount: $($invoiceResponse.data.totalAmount)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to generate invoice: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 3: Get invoice details
Write-Host "`n3. Retrieving Invoice Details..." -ForegroundColor Yellow

try {
    $invoiceDetails = Invoke-RestMethod -Uri "$baseUrl/invoices/$invoiceId" -Method GET
    Write-Host "✅ Invoice details retrieved!" -ForegroundColor Green
    Write-Host "   Status: $($invoiceDetails.data.status)" -ForegroundColor Cyan
    Write-Host "   Invoice Date: $($invoiceDetails.data.invoiceDate)" -ForegroundColor Cyan
    Write-Host "   Due Date: $($invoiceDetails.data.dueDate)" -ForegroundColor Cyan
    Write-Host "   Items Count: $($invoiceDetails.data.invoiceItems.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to retrieve invoice details: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get invoice as HTML
Write-Host "`n4. Generating Invoice HTML..." -ForegroundColor Yellow

try {
    $htmlResponse = Invoke-WebRequest -Uri "$baseUrl/invoices/$invoiceId/html" -Method GET
    $htmlContent = $htmlResponse.Content
    
    # Save HTML to file for viewing
    $htmlFile = "Invoice_$invoiceNumber.html"
    $htmlContent | Out-File -FilePath $htmlFile -Encoding UTF8
    
    Write-Host "✅ Invoice HTML generated successfully!" -ForegroundColor Green
    Write-Host "   HTML file saved as: $htmlFile" -ForegroundColor Cyan
    Write-Host "   File size: $($htmlContent.Length) characters" -ForegroundColor Cyan
    
    # Open HTML file in default browser
    if (Test-Path $htmlFile) {
        Start-Process $htmlFile
        Write-Host "   HTML file opened in browser for preview" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Failed to generate HTML: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test print endpoint
Write-Host "`n5. Testing Print Endpoint..." -ForegroundColor Yellow

try {
    $printRequest = @{
        format = "HTML"
        includeLogo = $true
        showTaxBreakdown = $true
    } | ConvertTo-Json

    $printResponse = Invoke-WebRequest -Uri "$baseUrl/invoices/$invoiceId/print" -Method POST -Body $printRequest -ContentType "application/json"
    $printHtml = $printResponse.Content
    
    # Save print version
    $printFile = "Invoice_Print_$invoiceNumber.html"
    $printHtml | Out-File -FilePath $printFile -Encoding UTF8
    
    Write-Host "✅ Print version generated successfully!" -ForegroundColor Green
    Write-Host "   Print file saved as: $printFile" -ForegroundColor Cyan
    
    # Open print version
    if (Test-Path $printFile) {
        Start-Process $printFile
        Write-Host "   Print version opened in browser" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Failed to generate print version: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: List all invoices
Write-Host "`n6. Listing All Invoices..." -ForegroundColor Yellow

try {
    $allInvoices = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method GET
    Write-Host "✅ Retrieved all invoices!" -ForegroundColor Green
    Write-Host "   Total invoices: $($allInvoices.data.Count)" -ForegroundColor Cyan
    
    foreach ($invoice in $allInvoices.data) {
        Write-Host "   - Invoice $($invoice.id): $($invoice.invoiceNumber) (Order: $($invoice.orderId), Status: $($invoice.status))" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Failed to retrieve invoices: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Update invoice status
Write-Host "`n7. Updating Invoice Status..." -ForegroundColor Yellow

try {
    $statusUpdate = "Sent"
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/invoices/$invoiceId/status" -Method PUT -Body $statusUpdate -ContentType "application/json"
    Write-Host "✅ Invoice status updated to: $statusUpdate" -ForegroundColor Green
    
    # Verify status update
    $updatedInvoice = Invoke-RestMethod -Uri "$baseUrl/invoices/$invoiceId" -Method GET
    Write-Host "   Verified status: $($updatedInvoice.data.status)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to update status: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== INVOICE SYSTEM TEST COMPLETE ===" -ForegroundColor Green
Write-Host "Generated Files:" -ForegroundColor Cyan
Write-Host "  - Invoice_$invoiceNumber.html (HTML version)" -ForegroundColor White
Write-Host "  - Invoice_Print_$invoiceNumber.html (Print version)" -ForegroundColor White
Write-Host "`nYou can now:" -ForegroundColor Yellow
Write-Host "  1. Open the HTML files in your browser" -ForegroundColor White
Write-Host "  2. Print them using Ctrl+P" -ForegroundColor White
Write-Host "  3. Use the print version for professional printing" -ForegroundColor White
Write-Host "  4. Customize the template in InvoiceService.cs" -ForegroundColor White

